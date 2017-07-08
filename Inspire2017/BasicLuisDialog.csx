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

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json.Linq;

// Abstract interface to a backoffice system.
public interface ICheckDataUsage
{
    // IDialogContext provides all details surrounding current conversation.
    // Return Task as method likely called in async context.
    Task<decimal> GetMegabytesRemainingThisMonth(IDialogContext context);
}

// Mock implementation of a system that reports user data consumption/remaining amount.
[Serializable]
public class MockUsage : ICheckDataUsage
{
    static Random random = new Random();
    public async Task<decimal> GetMegabytesRemainingThisMonth(IDialogContext context)
    {
        await Task.Delay(100); // simulate time needed to call external backoffice system
        return random.Next(1024, 10240);
    }
}

// For more information about this template visit http://aka.ms/azurebots-csharp-luis
[Serializable]
public class BasicLuisDialog : LuisDialog<object>
{
    private ICheckDataUsage usage;
    
    public BasicLuisDialog(ICheckDataUsage backoffice) : base(new LuisService(new LuisModelAttribute(Utils.GetAppSetting("LuisAppId"), Utils.GetAppSetting("LuisAPIKey"))))
    {
        usage = backoffice;
    }

    // demo context #1: user asking to check their data usage this month
    [LuisIntent("CheckData")]
    public async Task CheckDataIntent(IDialogContext context, LuisResult result)
    {
        decimal mb = await usage.GetMegabytesRemainingThisMonth(context);
        
        await context.PostAsync($"You have {mb} MB of data remaining this month.");
        context.Wait(MessageReceived);
    }

    // demo context #2: user's home internet connection is down
    [LuisIntent("InternetDown")]
    public async Task InternetDownIntent(IDialogContext context, LuisResult result)
    {
        await context.PostAsync($"Sorry to hear that your Internet is out. How long have you been unable to connect?");
        context.Wait(MessageReceived);
    }
    
    // demo context #3: user is telling us how long their internet has been down
    [LuisIntent("Duration")]
    public async Task DurationIntent(IDialogContext context, LuisResult result)
    {
        if (result.Entities.Count > 0 && result.Entities[0].Type == "builtin.datetimeV2.duration")
        {
            // extract duration value as seconds regardless of how the user typed it
            string val = result.Entities[0].Resolution["values"].ToString();
            var secondsDown = Convert.ToInt32(JArray.Parse(val).Last["value"]);
            if (secondsDown >= 7200)
            {
                // more than two hours
                await context.PostAsync($"Sorry your internet has been down for so long. I have marked your service request as urgent.");
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync($"Please wait a few more minutes while our technicians restore your service.");
                context.Wait(MessageReceived);
            }
        }
        else
        {
            await context.PostAsync($"Our technicians are currently working to fix your outage. Thank you for your patience.");
            context.Wait(MessageReceived);
        }
    }

    [LuisIntent("None")]
    public async Task NoneIntent(IDialogContext context, LuisResult result)
    {
        await context.PostAsync($"Sorry, I don't understand. Can you rephrase?");
        context.Wait(MessageReceived);
    }
}
