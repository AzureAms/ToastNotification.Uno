using System;

namespace Notification.FrameworkDependent
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
