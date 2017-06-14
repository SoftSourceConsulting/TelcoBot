/*  
 *  BillingDialog.cs
 *
 *  SoftSource Consulting, Inc.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using TelcoBot.Model;
using TelcoBot.Persistence;

namespace TelcoBot.Dialogs
{
    [Serializable]
    public class BillingDialog : TelcoBotDialog<Intent>
    {
        private readonly Intent[] _availableIntents = new[] { Intent.UpgradeService , Intent.ServiceInquiry };
        private readonly string _initialMonthEntity;
        private readonly string _initialPaymentMethodEntity;

        public BillingDialog(string baseImageAddress, string month = null, string paymentMethod = null)
            : base(baseImageAddress)
        {
            _initialMonthEntity = month;
            _initialPaymentMethodEntity = paymentMethod;
        }

        public override async Task StartAsync(IDialogContext context)
        {
            await base.StartAsync(context);

            if (string.IsNullOrEmpty(_initialMonthEntity))
                await ShowBills(context);
            else
                await HandlePaymentIntent(context, _initialMonthEntity, _initialPaymentMethodEntity);
        }

        private async Task ShowBills(IDialogContext context)
        {
            User user = GetUser(context);
            IEnumerable<Bill> bills = TelcoBotRepository.FindBillsByUser(user);
            if (bills.Any())
            {
                await PromptBills(context, bills);
            }
            else
            {
                await context.ReplyWithMessage(Properties.Resources.NoBillingRecordsMessage);
                context.Done(Intent.None);
            }
        }

        private async Task PromptBills(IDialogContext context, IEnumerable<Bill> bills)
        {
            bool isLatestBill = true;
            List<Attachment> attachments = new List<Attachment>();

            bills = bills.OrderByDescending(b => b.Year).ThenByDescending(b => b.Month);

            foreach (Bill bill in bills)
            {
                Attachment card = BuildCardForOneBill(bill, isLatestBill);
                attachments.Add(card);
                isLatestBill = false;
            }

            await context.ReplyWithMessage(null, attachments.ToArray());
            context.Wait(MessageReceivedAsync);
        }

        private Attachment BuildCardForOneBill(Bill bill, bool isLatestBill)
        {
            CardBuilder builder = new CardBuilder(BaseImageAddress, s => s + Environment.NewLine + bill.Id.ToString());

            string title = $"{bill.Month.AsMonth()} {bill.Year}";
            if (isLatestBill)
                title += " (" + Properties.Resources.CurrentLabel + ")";

            string subtitle = string.Format(Properties.Resources.InvoiceSubtitleTemplate, bill.Amount.AsDollars());
            if (bill.IsPaid)
                subtitle += " (" + Properties.Resources.PaidLabel + ")";
            else if (!isLatestBill)
                subtitle += " (" + Properties.Resources.PastDueLabel + ")";

            List<string> buttons = new List<string>() { Properties.Resources.InvoiceDetailLabel, Properties.Resources.GoBackLabel };
            if (!bill.IsPaid)
                buttons.Insert(0, string.Format(Properties.Resources.PayThisBillLabelTemplate, Properties.Resources.ThisMonthToken));

            Attachment result = builder.BuildHero(title, subtitle, "invoice2.png", buttons.ToArray());
            return result;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            IMessageActivity message = await argument;
            LuisResult interpretation = await Interpret(message);
            Intent intent = GetTopIntent(interpretation);

            if (intent == Intent.Pay)
            {
                string monthArg = GetTopEntity(interpretation, Properties.Resources.MonthEntityType)?.Entity;
                string paymentMethodArg = GetTopEntity(interpretation, Properties.Resources.PaymentMethodEntityType)?.Entity;

                await HandlePaymentIntent(context, monthArg, paymentMethodArg);
            }
            else if (intent == Intent.ViewInvoice)
            {
                await AnnounceLackOfSupport(context, Properties.Resources.InvoiceDetailProcessLabel);
                await ShowBills(context);
            }
            else if (intent == Intent.GoBack)
            {
                context.Done(Intent.None);
            }
            else if (_availableIntents.Contains(intent))
            {
                context.Done(intent);
            }
            else
            {
                await AnnounceIncomprehension(context);
                await ShowBills(context);
            }
        }

        private async Task PayBillCallback(IDialogContext context, IAwaitable<Intent> argument)
        {
            object test = await argument;
            Intent result = await argument;
            if (result == Intent.GoBack)
            {
                await ShowBills(context);
            }
            else
            {
                context.Done(Intent.None);
            }
        }

        private async Task HandlePaymentIntent(IDialogContext context, string monthEntity, string paymentMethodEntity)
        {
            User user = GetUser(context);   // should never be null at this point
            Bill bill = null;
            UserPaymentMethod paymentMethod = null;

            if (TryGetBill(user, monthEntity, out bill))
                TryGetPaymentMethod(user, paymentMethodEntity, out paymentMethod);

            if (bill == null)
            {
                await context.ReplyWithMessage(string.Format(Properties.Resources.NoMatchingUnpaidBillMessageTemplate, monthEntity));
                await ShowBills(context);
            }
            else
            {
                context.Call(new PayOneBillDialog(BaseImageAddress, bill, paymentMethod), PayBillCallback);
            }
        }

        private bool TryGetBill(User user, string month, out Bill bill)
        {
            bill = null;
            month = month?.Trim();

            IEnumerable<Bill> bills = TelcoBotRepository.FindBillsByUser(user).OrderByDescending(b => b.Year).ThenByDescending(b => b.Month);

            if (string.IsNullOrWhiteSpace(month))
            {
                // if no month is specified but there's exactly one unpaid bill
                if (bills.Count(b => !b.IsPaid) == 1)
                    bill = bills.Single(b => !b.IsPaid);
            }
            else if (month.Length >= 3)
            {
                if (month.Equals(Properties.Resources.ThisMonthToken, StringComparison.CurrentCultureIgnoreCase))
                {
                    bill = bills.FirstOrDefault();
                }
                else if (month.Equals(Properties.Resources.LastMonthToken, StringComparison.CurrentCultureIgnoreCase))
                {
                    bill = bills.Skip(1).FirstOrDefault();
                }
                else
                {
                    bill = bills.Take(12).FirstOrDefault(b => b.Month.AsMonth().StartsWith(month, StringComparison.CurrentCultureIgnoreCase));
                }
            }

            return bill != null;
        }

        private bool TryGetPaymentMethod(User user, string method, out UserPaymentMethod paymentMethod)
        {
            paymentMethod = null;
            method = method?.Trim();

            IEnumerable<UserPaymentMethod> methods = TelcoBotRepository.FindPaymentMethodsByUserId(user.Id);
            if (string.IsNullOrWhiteSpace(method))
            {
                if (methods.Count() == 1)
                    paymentMethod = methods.Single();
            }
            else
            {
                paymentMethod = methods.FirstOrDefault(m => m.Description.Equals(method, StringComparison.CurrentCultureIgnoreCase));
                if (paymentMethod == null)
                {
                    IEnumerable<UserPaymentMethod> candidates = methods.Where(m => m.Type.Equals(method, StringComparison.CurrentCultureIgnoreCase));
                    if (methods.Count() == 1)
                        paymentMethod = methods.Single();
                }
            }

            return paymentMethod != null;
        }
    }
}