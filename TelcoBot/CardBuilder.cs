/*  
 *  CardBuilder.cs
 *  Neil McKamey-Gonzalez
 *  Softsource Consulting, Inc.
 */

using System;
using System.Collections.Generic;
using Microsoft.Bot.Connector;

namespace TelcoBot
{
    public class CardBuilder
    {
        private readonly String _baseImageAddress;
        private readonly Func<string, string> _buttonLabelToMessage; 

        public CardBuilder(String baseImageAddress)
            : this(baseImageAddress, s => s)
        {
        }

        public CardBuilder(String baseImageAddress, Func<string, string> buttonLabelToMessageFunc)
        {
            _baseImageAddress = baseImageAddress;
            _buttonLabelToMessage = buttonLabelToMessageFunc;
        }

        public Attachment BuildHero(string prompt, string subTitle, string imageFile, params string[] buttonPrompts)
        {
            List<CardImage> cardImages = new List<CardImage>();

            if (!string.IsNullOrWhiteSpace(imageFile))
                cardImages.Add(new CardImage(url: _baseImageAddress + imageFile));

            HeroCard card = new HeroCard()
            {
                Title = prompt,
                Subtitle = subTitle,
                Images = cardImages,
                Buttons = GetButtons(buttonPrompts)
            };
            
            Attachment attachment = card.ToAttachment();

            return attachment;
        }

        public Attachment BuildThumbnail(string prompt, string subTitle, string imageFile, params string[] buttonPrompts)
        {
            List<CardImage> cardImages = new List<CardImage>();

            if (!string.IsNullOrWhiteSpace(imageFile))
                cardImages.Add(new CardImage(url: _baseImageAddress + imageFile));

            ThumbnailCard card = new ThumbnailCard()
            {
                Title = prompt,
                Subtitle = subTitle,
                Images = cardImages,
                Buttons = GetButtons(buttonPrompts),
            };

            Attachment attachment = card.ToAttachment();

            return attachment;
        }

        private List<CardAction> GetButtons(IEnumerable<string> buttonPrompts)
        {
            List<CardAction> cardButtons = new List<CardAction>();
            foreach (string buttonPrompt in buttonPrompts)
            {
                CardAction plButton = new CardAction()
                {
                    Title = buttonPrompt,
                    Type = "postBack",
                    Value = _buttonLabelToMessage(buttonPrompt)
                };

                cardButtons.Add(plButton);
            }
            return cardButtons;
        }
    }
}