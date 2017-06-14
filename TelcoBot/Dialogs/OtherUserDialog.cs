/*  
 *  OtherUserDialog.cs
 *
 *  SoftSource Consulting, Inc.
 */

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
    public class OtherUserDialog : TelcoBotDialog<Intent>
    {
        public OtherUserDialog(string baseImageAddress) 
            : base(baseImageAddress)
        {
        }

        public override async Task StartAsync(IDialogContext context)
        {
            await base.StartAsync(context);
            context.UserData.RemoveValue(UserKey);
            await PromptForName(context);
        }

        private async Task PromptForName(IDialogContext context)
        {
            Attachment attachment = new CardBuilder(BaseImageAddress).BuildHero("", Properties.Resources.WhatNamePrompt, null, Properties.Resources.NewUserLabel);
            await context.ReplyWithMessage(string.Empty, attachment);
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            IMessageActivity message = await argument;
            LuisResult interpretation = await Interpret(message);
            Intent intent = GetTopIntent(interpretation);

            if (intent == Intent.NewUser)
            {
                context.Done(Intent.NewUser);
            }
            else
            {
                string firstName;
                string lastName;
                if (message.Text.TryParseAsName(out firstName, out lastName))
                {
                    User user = TelcoBotRepository.FindUserByName(firstName, lastName);
                    if (user == null)
                    {
                        await context.ReplyWithMessage(Properties.Resources.NameNotFoundMessage);
                        await PromptForName(context);
                    }
                    else
                    {
                        context.UserData.SetValue(UserKey, user);
                        context.Done(Intent.None);
                    }
                }
                else
                {
                    await AnnounceIncomprehension(context);
                    await PromptForName(context);
                }
            }
        }
    }
}