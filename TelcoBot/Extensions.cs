/*  
 *  Extensions.cs
 *
 *  SoftSource Consulting, Inc.
 */

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