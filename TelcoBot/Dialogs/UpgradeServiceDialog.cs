/*  
 *  UpgradeServiceDialog.cs
 *
 *  SoftSource Consulting, Inc.
 */

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
    public class UpgradeServiceDialog : TelcoBotDialog<Intent>
    {
        private readonly Intent[] _availableIntents = new [] {Intent.GoBack, Intent.MainMenu, Intent.Billing, Intent.ServiceInquiry, Intent.UpgradeService};
        private const string ServiceTypeKey = "ServiceType";

        public UpgradeServiceDialog(string baseImageAddress)
            : base(baseImageAddress)
        {
        }

        public override async Task StartAsync(IDialogContext context)
        {
            await base.StartAsync(context);

            string serviceType;
            if (context.UserData.TryGetValue(ServiceTypeKey, out serviceType))
            {
                if (serviceType == Properties.Resources.TvLabel)
                {
                    await AnnounceLackOfSupport(context, Properties.Resources.TvServiceLabel, true);
                    await PromptForServiceType(context);
                }
                else
                {
                    await ShowInternetServiceLevels(context);
                }
            }
            else
            {
                await PromptForServiceType(context);
            }
        }

        private async Task PromptForServiceType(IDialogContext context)
        {
            context.UserData.RemoveValue(ServiceTypeKey);

            Attachment card = new CardBuilder(BaseImageAddress).BuildHero("", Properties.Resources.WhichServiceQuestion, null, Properties.Resources.TvLabel, Properties.Resources.InternetLabel, Properties.Resources.NevermindLabel);

            await context.ReplyWithMessage("", card);
            context.Wait(new ResumeAfter<IMessageActivity>(ServiceTypeResponseReceivedAsync));
        }

        private async Task ShowInternetServiceLevels(IDialogContext context)
        {

            User user = GetUser(context);
            IEnumerable<InternetServiceLevel> levels = TelcoBotRepository.FindAllInternetServiceLevels();
            InternetServiceLevel currentLevel = levels.FirstOrDefault(l => l.Id == user.InternetServiceLevelId);

            decimal currentPrice = 0;
            string levelSummary = Properties.Resources.NoServiceLabel;
            if (currentLevel != null)
            {
                levelSummary = $"{currentLevel.Name} ({currentLevel.Description})";
                currentPrice = currentLevel.Price;
            }

            await context.ReplyWithMessage(Properties.Resources.CurrentServiceHeader + ": " + levelSummary);

            List<Attachment> cards = new List<Attachment>();
            foreach (InternetServiceLevel level in levels.Where(l => l.Id != user.InternetServiceLevelId).ToArray())
            {
                CardBuilder builder = new CardBuilder(BaseImageAddress, b => b + Environment.NewLine + level.Id);
                string title = string.Format(Properties.Resources.InternetServiceTitleTemplate, level.Name, (level.Price - currentPrice).AsDollars());
                Attachment card = builder.BuildHero(title, level.Description, level.Image, Properties.Resources.UpgradeNowLabel, Properties.Resources.GoBackLabel);
                cards.Add(card);
            }

            await context.ReplyWithMessage("", cards.ToArray());
            context.Wait(InternetServiceLevelsResponseReceivedAsync);
        }

        private async Task ServiceTypeResponseReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            IMessageActivity message = await argument;

            if (message.Text.Equals(Properties.Resources.TvLabel, StringComparison.CurrentCultureIgnoreCase))
            {
                await AnnounceLackOfSupport(context, Properties.Resources.TvServiceLabel, true);
                await PromptForServiceType(context);
            }
            else if (message.Text.Equals(Properties.Resources.InternetLabel, StringComparison.CurrentCultureIgnoreCase))
            {
                context.UserData.SetValue(ServiceTypeKey, message.Text);
                await ShowInternetServiceLevels(context);
            }
            else
            {
                LuisResult interpretation = await Interpret(message);
                Intent intent = GetTopIntent(interpretation);
                if (_availableIntents.Contains(intent))
                {
                    context.Done(intent);
                }
                else
                {
                    await AnnounceIncomprehension(context);
                    await PromptForServiceType(context);
                }
            }
        }

        private async Task InternetServiceLevelsResponseReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            IMessageActivity message = await argument;

            string[] lines = message.Text.SplitList(Environment.NewLine);
            if (lines[0] == "Upgrade Now!")
            {
                int levelId;
                if (lines.Length < 2)
                {
                    // they must have typed "Upgrade Now!"
                    await context.ReplyWithMessage(Properties.Resources.UpgradePrompt);
                    await ShowInternetServiceLevels(context);
                }
                else if (int.TryParse(lines[1], out levelId))
                {
                    InternetServiceLevel level = TelcoBotRepository.FindInternetServiceLevelById(levelId);
                    User user = GetUser(context);
                    user.InternetServiceLevelId = levelId;
                    TelcoBotRepository.SaveChanges(user);
                    context.UserData.SetValue(UserKey, user);
                    context.UserData.RemoveValue(ServiceTypeKey);
                    await context.ReplyWithMessage(string.Format(Properties.Resources.UpgradeConfirmationTemplate, level.Name));
                    context.Done(Intent.MainMenu);
                }
                else
                {
                    throw new InvalidOperationException("Unable to parse the InternetServiceLevelId information");
                }
            }
            else
            {
                LuisResult interpretation = await Interpret(message);
                Intent intent = GetTopIntent(interpretation);
                if (_availableIntents.Contains(intent))
                {
                    context.Done(intent);
                }
                else
                {
                    context.UserData.RemoveValue(ServiceTypeKey);
                    await AnnounceIncomprehension(context);
                    await ShowInternetServiceLevels(context);
                }
            }
        }
    }
}