using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Natives
{
    internal enum NotificationIconNotify
    {
        BalloonShow = WindowMessage.User + 2,
        BalloonHide = WindowMessage.User + 3,
        BalloonTimeout = WindowMessage.User + 4,
        BalloonClick = WindowMessage.User + 5
    }
}
