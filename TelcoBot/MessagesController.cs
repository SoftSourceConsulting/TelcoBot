/*  
 *  MessageController.cs
 *  Neil McKamey-Gonzalez
 *  Softsource Consulting, Inc.
 */

 using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using TelcoBot.Dialogs;

namespace TelcoBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            string idInChannel = activity.From.Id;      // unique user Id, not the actual id in the channel, like a skype ID.

            // we don't currently handle any other sorts of activities
            if (activity.Type == ActivityTypes.Message)
            {
                string baseImageAddress = Url.Request.RequestUri.AbsoluteUri.Replace(@"api/messages", string.Empty) + @"images/";
                await Conversation.SendAsync(activity, () => new MasterDialog(baseImageAddress, idInChannel));
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
    }
}