using System;

namespace Notification.Natives
{
    [Flags]
    internal enum NotificationIconFlags
    {
        /// <summary>
        /// 0x00000001. The uCallbackMessage member is valid.
        /// </summary>
        Message = 0x00000001,
        /// <summary>
        /// 0x00000002. The hIcon member is valid.
        /// </summary>
        Icon = 0x00000002,
        /// <summary>
        /// 0x00000004. The szTip member is valid.
        /// </summary>
        Tip = 0x00000004,
        /// <summary>
        /// 0x00000008. The dwState and dwStateMask members are valid.
        /// </summary>
        State = 0x00000008,
        /// <summary>
        /// 0x00000010. Display a balloon notification. The szInfo, szInfoTitle, dwInfoFlags, and uTimeout members are valid. Note that uTimeout is valid only in Windows 2000 and Windows XP.
        /// </summary>
        Info = 0x00000010,
        /// <summary>
        /// The guidItem is valid.
        /// </summary>
        Guid = 0x00000020,
        /// <summary>
        ///  If the balloon notification cannot be displayed immediately, discard it. Use this flag for notifications that represent real-time information which would be meaningless or misleading if displayed at a later time. For example, a message that states "Your telephone is ringing." NIF_REALTIME is meaningful only when combined with the NIF_INFO flag.
        /// </summary>
        Realtime = 0x00000040,
        /// <summary>
        /// Use the standard tooltip. Normally, when uVersion is set to NOTIFYICON_VERSION_4, the standard tooltip is suppressed and can be replaced by the application-drawn, pop-up UI. If the application wants to show the standard tooltip with NOTIFYICON_VERSION_4, it can specify NIF_SHOWTIP to indicate the standard tooltip should still be shown.
        /// </summary>
        Showtip = 0x00000080
    }
}
