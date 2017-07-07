using System;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

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

    // Anything not understood by LUIS goes here.
    [LuisIntent("None")]
    public async Task NoneIntent(IDialogContext context, LuisResult result)
    {
        decimal mb = await usage.GetMegabytesRemainingThisMonth(context);
        
        await context.PostAsync($"You have {mb} MB of data remaining this month."); //
        context.Wait(MessageReceived);
    }

    // Go to https://luis.ai and create a new intent, then train/publish your luis app.
    // Finally replace "MyIntent" with the name of your newly created intent in the following handler
    [LuisIntent("InternetOutage")]
    public async Task InternetOutageIntent(IDialogContext context, LuisResult result)
    {
        await context.PostAsync($"Sorry to hear that your Internet is out. Would you please answer a few questions so our service technicians can expedite a solution?"); //
        context.Wait(MessageReceived);
    }
}
