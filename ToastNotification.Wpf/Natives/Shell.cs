using System.Runtime.InteropServices;

namespace Notification.Natives
{
    internal static class Shell
    {
        /// <summary>
        /// Sends a message to the taskbar's status area.
        /// https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shell_notifyiconw
        /// </summary>
        [DllImport("Shell32.dll", EntryPoint = "Shell_NotifyIconW")]
        static extern bool NotifyIconW(NotificationIconMessage dwMessage, [In] ref NotificationIconData lpData);

        [DllImport("Kernel32.dll", EntryPoint = "GetLastError")]
        public static extern uint GetLastError();

        public static bool NotifyIcon(NotificationIconMessage message, NotificationIconData data)
        {
            return NotifyIconW(message, ref data);
        }
    }
}
