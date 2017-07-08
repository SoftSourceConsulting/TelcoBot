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

namespace TelcoBot.Dialogs
{
    [Serializable]
    public class MainMenuDialog : TelcoBotDialog<Intent>
    {
        public MainMenuDialog(string baseImageAddress) 
            : base(baseImageAddress)
        {
        }

        public override async Task StartAsync(IDialogContext context)
        {
            await base.StartAsync(context);

            Attachment attachment = new CardBuilder(BaseImageAddress).BuildHero(Properties.Resources.MainMenuTitle, Properties.Resources.MainMenuDirections, "Person2.png", Properties.Resources.BillingLabel, Properties.Resources.ServiceInquiryLabel, Properties.Resources.UpgradeLabel);
            await context.ReplyWithMessage(null, attachment);
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            IMessageActivity message = await argument;

            LuisResult interpretation = await Interpret(message);
            Intent intent = GetTopIntent(interpretation);

            context.Done(intent);
        }
    }
}