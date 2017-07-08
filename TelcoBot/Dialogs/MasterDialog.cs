// 
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
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using TelcoBot.Model;
using TelcoBot.Persistence;

namespace TelcoBot.Dialogs
{
    [Serializable]
    public class MasterDialog : TelcoBotDialog<object>
    {
        private readonly string _userIdInChannel;

        private Intent _initialIntent = Intent.None;
        private string _initialMonthEntity = null;
        private string _initialPaymentMethodEntity = null;
        private bool _recentlyReset = false;

        public MasterDialog(string baseImageAddress, string initialUserIdInChannel) 
            : base(baseImageAddress)
        {
            _userIdInChannel = initialUserIdInChannel;
        }

        public override async Task StartAsync(IDialogContext context)
        {
            await base.StartAsync(context);

            CardBuilder builder = new CardBuilder(BaseImageAddress);
            Attachment card = builder.BuildHero("", "", Properties.Settings.Default.VendorLogoImageFile);
            await context.ReplyWithMessage("", card);

            if (_recentlyReset)
            {
                _recentlyReset = false;
                context.Call(new LoginDialog(BaseImageAddress, _userIdInChannel), LoginCallback);
            }
            else
            {
                context.Wait(InitialMessageReceivedAsync);
            }
        }

        private async Task InitialMessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            IMessageActivity message = await argument;
            LuisResult interpretation = await Interpret(message);

            _initialIntent = GetTopIntent(interpretation);
            _initialMonthEntity = GetTopEntity(interpretation, Properties.Resources.MonthEntityType)?.Entity;
            _initialPaymentMethodEntity = GetTopEntity(interpretation, Properties.Resources.PaymentMethodEntityType)?.Entity;

            context.Call(new LoginDialog(BaseImageAddress, _userIdInChannel), LoginCallback);
        }

        private async Task LoginCallback(IDialogContext context, IAwaitable<Intent> result)
        {
            // The login dialog is a dead end until there's a valid user selected or created.
            // So by the time we get here, there will always be a user in context.UserData
            User user = GetUser(context);
            if (user == null)
                throw new InvalidOperationException("By the end of the login process, the user should be non-null.");

            SetIdInChannel(user, _userIdInChannel);

            if (_initialIntent == Intent.None || _initialIntent == Intent.Greeting)
                await context.ReplyWithMessage(Properties.Resources.MainMenuMessage);

            Intent intent = _initialIntent;
            string month = _initialMonthEntity;
            string paymentMethod = _initialPaymentMethodEntity;

            _initialIntent = Intent.None;
            _initialMonthEntity = null;
            _initialPaymentMethodEntity = null;

            await ExecuteAppropriately(context, intent, month, paymentMethod);
        }

        private void SetIdInChannel(User target, string idInChannel)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            while (true)
            {
                User user = TelcoBotRepository.FindUserByIdInChannel(idInChannel);
                if (user == null)
                {
                    break;
                }
                else
                {
                    user.IdInChannel = string.Empty;
                    TelcoBotRepository.SaveChanges(user);
                }
            }

            target.IdInChannel = idInChannel;
            TelcoBotRepository.SaveChanges(target);
        }

        private async Task MainMenuCallback(IDialogContext context, IAwaitable<Intent> result)
        {
            // TODO we have to get the full intent interpretation from the result
            Intent intent = await result;
            await ExecuteAppropriately(context, intent);
        }

        private async Task ExecuteAppropriately(IDialogContext context, Intent intent = Intent.None, string monthEntity = null, string paymentMethodEntity = null)
        {
            if (intent == Intent.None || intent == Intent.Greeting)
            {
                context.Call(new MainMenuDialog(BaseImageAddress), MainMenuCallback);
            }
            else if (intent == Intent.Pay)
            {
                context.Call(new BillingDialog(BaseImageAddress, monthEntity, paymentMethodEntity), MainMenuCallback);
            }
            else if (intent == Intent.ServiceInquiry)
            {
                await AnnounceLackOfSupport(context, Properties.Resources.ServiceInquiriesLabel, true);
                context.Call(new MainMenuDialog(BaseImageAddress), MainMenuCallback);
            }
            else if (intent == Intent.UpgradeService)
            {
                context.Call(new UpgradeServiceDialog(BaseImageAddress), MainMenuCallback);
            }
            else if (intent == Intent.Billing)
            {
                context.Call(new BillingDialog(BaseImageAddress), MainMenuCallback);
            }
            else if (intent == Intent.Reset)
            {
                await context.ReplyWithMessage(Properties.Resources.ConversationReset);
                context.UserData.Clear();
                context.PrivateConversationData.Clear();
                context.Done(null as object);
                _recentlyReset = true;
            }
            else
            {
                context.Call(new MainMenuDialog(BaseImageAddress), MainMenuCallback);
            }
        }
    }
}