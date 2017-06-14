/*  
 *  ConfirmIdentityDialog.cs
 *
 *  SoftSource Consulting, Inc.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using TelcoBot.Model;

namespace TelcoBot.Dialogs
{
    [Serializable]
    public class ConfirmIdentityDialog : TelcoBotDialog<Intent>
    {
        public ConfirmIdentityDialog(string baseImageAddress)
            : base(baseImageAddress)
        {
        }

        public override async Task StartAsync(IDialogContext context)
        {
            await base.StartAsync(context);

            await PromptForIdentityConfirmation(context);
        }

        private async Task PromptForIdentityConfirmation(IDialogContext context)
        {
            // there should always be a non-null user at this point
            User user = GetUser(context);

            string prompt = string.Format(Properties.Resources.NameAssociationPromptTemplate, user.FirstName, user.LastName, Properties.Settings.Default.VendorName);

            Attachment attachment = new CardBuilder(BaseImageAddress).BuildHero("", prompt, null, Properties.Resources.MatchYesLabel, Properties.Resources.MatchNoLabel);
            await context.ReplyWithMessage(null, attachment);
            context.Wait(new ResumeAfter<IMessageActivity>(ConfirmIdentityResponseReceivedAsync));
        }

        protected async Task ConfirmIdentityResponseReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            IMessageActivity message = await argument;

            LuisResult interpretation = await Interpret(message);
            Intent intent = GetTopIntent(interpretation);

            if (intent == Intent.Yes)
            {
                context.Done(Intent.Yes);
            }
            else if (intent == Intent.No)
            {
                context.Done(Intent.OtherUser);
            }
            else
            {
                await AnnounceIncomprehension(context);
                await PromptForIdentityConfirmation(context);
            }
        }
    }
}