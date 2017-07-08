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

namespace TelcoBot.Dialogs
{
    // Exit conditions: got good pin (Intents.None) or Intents.OtherUser or Intents.NewUser
    [Serializable]
    public class PINDialog : TelcoBotDialog<Intent>
    {
        public PINDialog(string baseImageAddress)
            : base(baseImageAddress)
        {
        }

        public override async Task StartAsync(IDialogContext context)
        {
            await base.StartAsync(context);
            await PromptForPIN(context);
        }

        private async Task PromptForPIN(IDialogContext context)
        {
            User user = GetUser(context);

            Attachment attachment = new CardBuilder(BaseImageAddress).BuildHero("", Properties.Resources.PinPrompt, null, string.Format(Properties.Resources.NotMeLabelTemplate, user.FirstName, user.LastName));
            await context.ReplyWithMessage(null, attachment);
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            IMessageActivity message = await argument;
            // if we got to here, the user should be non-null
            User user = GetUser(context);
            int pin = 0;

            if (int.TryParse(message.Text, out pin))
            {
                if (pin == user.PIN)
                {
                    context.Done(Intent.None);
                }
                else
                {
                    await context.ReplyWithMessage(Properties.Resources.IncorrectPinMessage);
                    await PromptForPIN(context);
                }
            }
            else
            {
                LuisResult interpretation = await Interpret(message);
                Intent intent = GetTopIntent(interpretation);

                if (intent == Intent.NewUser)
                {
                    context.Done(intent);
                }
                else if (intent == Intent.OtherUser)
                {
                    context.Done(intent);
                }
                else
                {
                    await AnnounceIncomprehension(context);
                    await PromptForPIN(context);
                }
            }
        }
    }
}