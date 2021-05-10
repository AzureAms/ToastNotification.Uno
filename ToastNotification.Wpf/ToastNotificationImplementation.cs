using Microsoft.Win32;
using Notification.Natives;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Uno.Extras
{
    public static class ToastNotificationImplementation
    {
        private static readonly object dummy = new object();
        private static Guid id = id = Guid.NewGuid();

        /// <summary>
        /// Shows the toast notification using native Win32 APIs.
        /// </summary>
        /// <param name="toast">ToatsNotification as created in the native library.</param>
        public static async Task Show(this ToastNotification toast)
        {
            Icon icon;
            if (toast.AppLogoOverride != null)
            {
                var imageStream = await toast.AppLogoOverride.GetStreamAsync().ConfigureAwait(false);
                var bitmap = (Bitmap)Image.FromStream(imageStream);
                icon = Icon.FromHandle(bitmap.GetHicon());
            }
            else
            {
                icon = GetProcessIcon();
            }

            lock (dummy)
            {
                var notifyData = new NotificationIconData();
                notifyData.hWnd = Process.GetCurrentProcess().MainWindowHandle;
                notifyData.cbSize = (uint)Marshal.SizeOf(notifyData);
                notifyData.DUMMYUNIONNAME_uTimeout_uVersion = 0x4;
                notifyData.hIcon = GetProcessIcon().Handle;
                notifyData.szTip = string.Empty;
                notifyData.uFlags = NotificationIconFlags.Icon | NotificationIconFlags.Tip | NotificationIconFlags.Guid;
                notifyData.guidItem = id;

                notifyData.AddIcon();

                notifyData.uFlags = NotificationIconFlags.Info | NotificationIconFlags.Icon | NotificationIconFlags.Guid;
                notifyData.szInfoTitle = toast.Title;
                notifyData.szInfo = toast.Message;
                notifyData.dwInfoFlags = NotificationIconInfoFlags.User | NotificationIconInfoFlags.LargeIcon;
                notifyData.hBalloonIcon = icon.Handle;

                Shell.NotifyIcon(NotificationIconMessage.Modify, notifyData);

                // Extra 1500ms for the notification to fade out:
                Thread.Sleep(GetNotificationDuration() * 1000 + 1500);
                notifyData.RemoveIcon();
            }
        }

        private static void AddIcon(this NotificationIconData notifyData)
        {
            Shell.NotifyIcon(NotificationIconMessage.Add, notifyData);
            Shell.NotifyIcon(NotificationIconMessage.SetVersion, notifyData);
        }

        private static void RemoveIcon(this NotificationIconData notifyData)
        {
            Shell.NotifyIcon(NotificationIconMessage.Delete, notifyData);
        }

        /// <summary>
        /// Gets the duration of notifications in seconds.
        /// </summary>
        /// <returns>The duration of notification, in seconds.</returns>
        private static int GetNotificationDuration()
        {
            return Registry.CurrentUser.OpenSubKey("Control Panel")
                                      ?.OpenSubKey("Accessibility")
                                      ?.GetValue("MessageDuration") as int?
                                      ?? 9;
        }

        private static Icon GetProcessIcon()
        {
            Icon icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
            return icon;
        }

  
    }
}
