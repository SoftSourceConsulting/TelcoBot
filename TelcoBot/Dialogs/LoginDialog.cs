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
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using TelcoBot.Model;
using TelcoBot.Persistence;

namespace TelcoBot.Dialogs
{
    [Serializable]
    public class LoginDialog : TelcoBotDialog<Intent>
    {
        private readonly string _userIdInChannel;

        public LoginDialog(string baseImageAddress, string userIdInChannel) 
            : base(baseImageAddress)
        {
            _userIdInChannel = userIdInChannel;
        }

        public override async Task StartAsync(IDialogContext context)
        {
            await base.StartAsync(context);

            User userCandidate = TelcoBotRepository.FindUserByIdInChannel(_userIdInChannel);

            if (userCandidate == null)
            {
                context.Call(new OtherUserDialog(BaseImageAddress), OtherUserCallback);
            }
            else
            {
                context.UserData.SetValue(UserKey, userCandidate);
                context.Call(new ConfirmIdentityDialog(BaseImageAddress), new ResumeAfter<Intent>(IdentityConfirmationCallBack));
            }
        }

        private async Task IdentityConfirmationCallBack(IDialogContext context, IAwaitable<Intent> argument)
        {
            Intent intent = await argument;
            if (intent == Intent.Yes)
            {
                context.Call(new PINDialog(BaseImageAddress), PinPromptCallback);
            }
            else if (intent == Intent.OtherUser)
            {
                context.Call(new OtherUserDialog(BaseImageAddress), OtherUserCallback);
            }
            else
            {
                // we should never get here, but just in case...
                await AnnounceIncomprehension(context);
                context.Call(new ConfirmIdentityDialog(BaseImageAddress), new ResumeAfter<Intent>(IdentityConfirmationCallBack));
            }
        }

        private async Task OtherUserCallback(IDialogContext context, IAwaitable<Intent> argument)
        {
            Intent result = await argument;
            User user = GetUser(context);

            if (result == Intent.NewUser)
            {
                context.Call(new NewUserDialog(BaseImageAddress), NewUserCallback);
            }
            else if (user == null)
            {
                throw new InvalidOperationException("Invalid state after 'other user' dialog.");
            }
            else
            {
                context.Call(new ConfirmIdentityDialog(BaseImageAddress), new ResumeAfter<Intent>(IdentityConfirmationCallBack));
            }
        }

        private async Task NewUserCallback(IDialogContext context, IAwaitable<Intent> result)
        {
            User user = GetUser(context);
            if (user == null)
                context.Call(new OtherUserDialog(BaseImageAddress), OtherUserCallback);
            else
                context.Done(Intent.None);
        }

        private async Task PinPromptCallback(IDialogContext context, IAwaitable<Intent> result)
        {
            Intent intent = await result;

            if (intent == Intent.None)
            {
                context.Done(Intent.None);
            }
            else if (intent == Intent.NewUser)
            {
                context.Call(new NewUserDialog(BaseImageAddress), NewUserCallback);
            }
            else if (intent == Intent.OtherUser)
            {
                context.Call(new OtherUserDialog(BaseImageAddress), OtherUserCallback);
            }
            else
            {
                // any other possibilities should have caused looping in PINDialog
                throw new InvalidOperationException("Unexpected intent: " + intent);
            }
        }
    }
}