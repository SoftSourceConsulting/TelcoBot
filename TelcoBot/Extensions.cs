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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace TelcoBot
{
    public static class Extensions
    {
        public static async Task ReplyWithMessage(this IDialogContext context, string message, params Attachment[] attachments)
        {
            IMessageActivity reply = context.MakeMessage();
            reply.Text = message;
            reply.AttachmentLayout = "carousel";
            reply.Attachments = attachments.ToList();
            await context.PostAsync(reply);
        }

        public static string AsMonth(this int monthNumber)
        {
            if (monthNumber < 1 || monthNumber > 12)
                throw new ArgumentOutOfRangeException(nameof(monthNumber));

            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNumber);
        }

        public static string AsDollars(this decimal amount)
        {
            return amount.ToString("$#.00");
        }

        public static bool TryParseAsName(this string source, out string first, out string last)
        {
            bool canParse = false;
            first = null;
            last = null;

            if (!string.IsNullOrWhiteSpace(source))
            {
                bool swapNames = source.Contains(",");

                string[] parts = source.Split(new char[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 1)
                {
                    canParse = true;
                    first = parts[0];
                }
                else if (parts.Length > 1)
                {
                    canParse = true;
                    if (swapNames)
                    {
                        first = parts[1];
                        last = parts[0];
                    }
                    else
                    {
                        first = parts[0];
                        last = parts.Last();
                    }

                    first = first.Trim();
                    last = last.Trim();
                }
            }

            return canParse;
        }
    }
}