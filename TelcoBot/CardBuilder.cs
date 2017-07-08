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