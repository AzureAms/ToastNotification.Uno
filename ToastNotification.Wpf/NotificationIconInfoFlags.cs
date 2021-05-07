using System;

namespace Notification.Natives
{
    [Flags]
    internal enum NotificationIconInfoFlags
    {
        /// <summary>
        /// 0x00000000. No icon.
        /// </summary>
        None = 0x00000000,
        /// <summary>
        /// 0x00000001. An information icon.
        /// </summary>
        Info = 0x00000001,
        /// <summary>
        /// 0x00000002. A warning icon.
        /// </summary>
        Warning = 0x00000002,
        /// <summary>
        /// 0x00000003. An error icon.
        /// </summary>
        Error = 0x00000003,
        /// <summary>
        /// 0x00000004. Use the icon identified in hBalloonIcon as the notification balloon's title icon.
        /// </summary>
        User = 0x00000004,
        /// <summary>
        /// 0x00000010. Windows XP and later. Do not play the associated sound. Applies only to notifications.
        /// </summary>
        NoSound = 0x00000010,
        /// <summary>
        /// The large version of the icon should be used as the notification icon. This corresponds to the icon with dimensions SM_CXICON x SM_CYICON. If this flag is not set, the icon with dimensions XM_CXSMICON x SM_CYSMICON is used.
        /// </summary>
        LargeIcon = 0x00000020
    }
}
