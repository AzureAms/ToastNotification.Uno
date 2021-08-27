using System;
using System.Collections.Generic;
using System.Text;

namespace Notification
{
    internal static class Helpers
    {
        public static void CleanEvent(this EventHandler eventHandler)
        {
            if (eventHandler == null)
            {
                return;
            }

            foreach (var handler in eventHandler.GetInvocationList())
            {
                eventHandler -= (EventHandler)handler;
            }
        }
    }
}
