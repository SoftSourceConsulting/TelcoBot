/*  
 *  TelcoBotDialog.cs
 *
 *  SoftSource Consulting, Inc.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Resource;
using Microsoft.Bot.Connector;
using TelcoBot.Model;

namespace TelcoBot.Dialogs
{
    [Serializable]
    public abstract class TelcoBotDialog<TResult> : IDialog<TResult>
    {
        private const double DefaultMinimumConfidence = 0.05;

        protected const string UserKey = "User";
        protected const string PaymentMethodKey = "PaymentMethod";

        protected TelcoBotDialog(string baseImageAddress)
        {
            BaseImageAddress = baseImageAddress;
        }

        protected string BaseImageAddress { get; }

        public virtual async Task StartAsync(IDialogContext context)
        {
        }

        protected async Task AnnounceIncomprehension(IDialogContext context)
        {
            await context.ReplyWithMessage(Properties.Resources.IncomprehensionMessage);
        }

        protected User GetUser(IDialogContext context)
        {
            User user = null;

            if (!context.UserData.TryGetValue(UserKey, out user))
                user = null;

            return user;
        }

        protected async Task AnnounceLackOfSupport(IDialogContext context, string featureDescription, bool isPlural = false)
        {
            CardBuilder builder = new CardBuilder(BaseImageAddress);

            string template = isPlural ? Properties.Resources.NotSupportedTemplatePlural : Properties.Resources.NotSupportedTemplateSingular;
            Attachment card = builder.BuildHero(string.Format(template, featureDescription), "", "UnderConstruction2.png");

            await context.ReplyWithMessage("", card);
        }

        protected async Task<LuisResult> Interpret(IMessageActivity source)
        {
            // LUIS queries are handled here instead of with classes derived from LuisDialog because that would require hard-coding 
            //  the LUIS model ID and subscription key into the class definition, which are not appropriate for this implementation.

            string message = source.Text.SplitList(Environment.NewLine).FirstOrDefault();
            LuisService service = new LuisService(new LuisModelAttribute(Properties.Settings.Default.LuisModelId, Properties.Settings.Default.LuisSubscriptionKey));
            LuisResult result = await service.QueryAsync(message);

            return result;
        }

        protected Intent GetTopIntent(LuisResult results, double mininumConfidence = DefaultMinimumConfidence)
        {
            Intent result;

            IntentRecommendation intent = results.Intents.Where(i => i.Score.HasValue && i.Score.Value >= mininumConfidence).OrderByDescending(i => i.Score).FirstOrDefault();

            if (intent == null)
                result = Intent.None;
            else if (!Enum.TryParse<Intent>(intent.Intent, true, out result))
                throw new InvalidOperationException($"Unrecognized intent: {intent.Intent}");

            return result;
        }

        protected EntityRecommendation GetTopEntity(LuisResult results, double mininumConfidence = DefaultMinimumConfidence)
        {
            EntityRecommendation result = results.Entities.Where(e => e.Score.HasValue && e.Score.Value >= mininumConfidence).OrderByDescending(e => e.Score).FirstOrDefault();
            return result;
        }

        protected EntityRecommendation GetTopEntity(LuisResult results, string type, double mininumConfidence = DefaultMinimumConfidence)
        {
            EntityRecommendation result = results.Entities.Where(e => e.Type == type).Where(e => e.Score.HasValue && e.Score.Value >= mininumConfidence).OrderByDescending(e => e.Score).FirstOrDefault();
            return result;
        }
    }
}