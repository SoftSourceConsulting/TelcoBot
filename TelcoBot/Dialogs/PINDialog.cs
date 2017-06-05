/*  
 *  PINDialog.cs
 *  Neil McKamey-Gonzalez
 *  Softsource Consulting, Inc.
 */

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