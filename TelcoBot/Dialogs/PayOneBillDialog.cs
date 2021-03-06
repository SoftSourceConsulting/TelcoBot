﻿// 
// Copyright (c) SoftSource Consulting, Inc. All rights reserved.
// Licensed under the MIT license.
// 
// https://github.com/SoftSourceConsulting/TelcoBot
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Resource;
using Microsoft.Bot.Connector;
using TelcoBot.Model;
using TelcoBot.Persistence;

namespace TelcoBot.Dialogs
{
    [Serializable]
    public class PayOneBillDialog : TelcoBotDialog<Intent>
    {
        private readonly Bill _bill;
        private UserPaymentMethod _method;

        public PayOneBillDialog(string baseImageAddress, Bill bill, UserPaymentMethod paymentMethod = null)
            : base(baseImageAddress)
        {
            if (bill == null)
                throw new ArgumentNullException(nameof(bill));

            _bill = bill;
            _method = paymentMethod;
        }

        public override async Task StartAsync(IDialogContext context)
        {
            await base.StartAsync(context);

            if (_bill.IsPaid)
            {
                await context.ReplyWithMessage(Properties.Resources.BillAlreadyPaidMessage);
                context.Done(Intent.GoBack);
            }
            else if (_method == null)
            {
                await context.ReplyWithMessage(Properties.Resources.PaymentMethodPrompt);
                await PromptForPaymentMethod(context);
            }
            else
            {
                await PromptForFinalConfirmation(context);
            }
        }

        private async Task PromptForFinalConfirmation(IDialogContext context)
        {
            // we should get here only if both _bill and _method are nonnull

            string message = string.Format(Properties.Resources.FinalPaymentConfirmationPromptTemplate,
                                           _bill.Month.AsMonth(),
                                           _bill.Year,
                                           _bill.Amount.AsDollars(),
                                           _method.Description);
                
            CardBuilder builder = new CardBuilder(BaseImageAddress);
            Attachment card = builder.BuildHero("", message, null, string.Format(Properties.Resources.FinalPaymentConfirmationLabel, _bill.Amount.AsDollars()), Properties.Resources.NevermindLabel);

            await context.ReplyWithMessage("", card);
            context.Wait(FinalPromptResponseReceivedAsync);
        }

        protected async Task FinalPromptResponseReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            IMessageActivity message = await argument;
            LuisResult interpretation = await Interpret(message);
            Intent intent = GetTopIntent(interpretation);
            if (intent == Intent.Yes || intent == Intent.Pay)
            {
                await PayBill(context);
            }
            else if (intent == Intent.No || intent == Intent.GoBack)
            {
                context.Done(intent);
            }
            else
            {
                await AnnounceIncomprehension(context);
                await PromptForFinalConfirmation(context);
            }
        }

        protected async Task PaymentMethodResponseReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            IMessageActivity message = await argument;
            LuisResult interpretation = await Interpret(message);
            Intent intent = GetTopIntent(interpretation);

            if (intent == Intent.Pay)
            {
                string methodIdStr = message.Text.SplitList(Environment.NewLine).LastOrDefault();
                int methodId;
                if (int.TryParse(methodIdStr, out methodId))
                {
                    _method = TelcoBotRepository.FindUserPaymentMethodById(methodId);
                    await PayBill(context);
                }
                else
                {
                    await AnnounceIncomprehension(context);
                    context.Done(Intent.GoBack);
                }
            }
            else if (intent == Intent.GoBack)
            {
                context.Done(Intent.GoBack);
            }
            else
            {
                await AnnounceIncomprehension(context);
                context.Done(Intent.GoBack);
            }
        }

        private async Task PromptForPaymentMethod(IDialogContext context)
        {
            // user should never be null at this pointD:\Visual Studio Projects\TelcoBot\TelcoBot\Dialogs\BillingDialog.cs
            User user = GetUser(context);
            IEnumerable<UserPaymentMethod> methods = TelcoBotRepository.FindPaymentMethodsByUserId(user.Id);
            if (methods.Any())
            {
                List<Attachment> attachments = new List<Attachment>();

                foreach (UserPaymentMethod method in methods)
                {
                    Attachment card = BuildCardForOneMethod(method);
                    attachments.Add(card);
                }

                await context.ReplyWithMessage(null, attachments.ToArray());
                context.Wait(PaymentMethodResponseReceivedAsync);
            }
            else
            {
                await context.ReplyWithMessage(Properties.Resources.NoPaymentMethodsMessage);
                context.Done(Intent.GoBack);
            }
        }

        private async Task PayBill(IDialogContext context)
        {
            // we should get here only if both _bill and _method are nonnull

            _bill.IsPaid = true;

            TelcoBotRepository.SaveChanges(_bill);
            await context.ReplyWithMessage(string.Format(Properties.Resources.ThanksForPaymentMessageTemplate, _bill.Amount.AsDollars()));
            context.Done(Intent.None);
        }

        private Attachment BuildCardForOneMethod(UserPaymentMethod method)
        {
            CardBuilder builder = new CardBuilder(BaseImageAddress, s => s + Environment.NewLine + method.Id.ToString());

            Attachment result = builder.BuildHero(method.Description, $"{method.Type} {method.Identifier}", method.Image, Properties.Resources.PayNowLabel, Properties.Resources.GoBackLabel);
            return result;
        }
    }
}