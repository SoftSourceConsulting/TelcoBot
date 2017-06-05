/*  
 *  NewUserDialog.cs
 *  Neil McKamey-Gonzalez
 *  Softsource Consulting, Inc.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace TelcoBot.Dialogs
{
    [Serializable]
    public class NewUserDialog : TelcoBotDialog<Intent>
    {
        public NewUserDialog(string baseImageAddress) 
            : base(baseImageAddress)
        {
        }

        public override async Task StartAsync(IDialogContext context)
        {
            context.UserData.RemoveValue(UserKey);
            await base.StartAsync(context);
            await AnnounceLackOfSupport(context, Properties.Resources.AddingNewUsersProcessLabel);
            context.Done(Intent.None);
        }
    }
}