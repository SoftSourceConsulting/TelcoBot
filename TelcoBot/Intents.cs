/*  
 *  Intents.cs
 *  Neil McKamey-Gonzalez
 *  Softsource Consulting, Inc.
 */

using System;

namespace TelcoBot
{
    [Serializable]
    public enum Intent
    {
        // None has to be first so it will be the default value returned if no other intent is recognized
        None,
        Yes,
        No,
        NewUser,
        OtherUser,
        Greeting,
        Pay,
        Billing,
        ServiceInquiry,
        UpgradeService,
        ViewInvoice,
        GoBack,
        MainMenu,
        Reset
    }
}