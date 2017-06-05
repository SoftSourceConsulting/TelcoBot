/*  
 *  MainMenuDialog.cs
 *  Neil McKamey-Gonzalez
 *  Softsource Consulting, Inc.
 */

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