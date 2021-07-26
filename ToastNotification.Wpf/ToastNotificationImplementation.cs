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
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private static Guid id = id = Guid.NewGuid();

        private static uint _callbackMessage = (uint)WindowMessage.App + 1;
        /// <summary>
        /// The CallbackMessage, which is an application-defined value.
        /// The value MUST be between WindowMessage.App and 0xBFFF
        /// </summary>
        public static uint CallbackMessage
        {
            get => _callbackMessage;
            set
            {
                if (Math.Min(Math.Max((uint)WindowMessage.App, value), 0xBFFFu) != value)
                {
                    throw new ArgumentOutOfRangeException("Callback message code MUST be between WindowMessage.App and 0xBFFF");
                }
            }
        }

        /// <summary>
        /// Shows the toast notification using native Win32 APIs.
        /// </summary>
        /// <param name="toast">ToatsNotification as created in the native library.</param>
        public static async Task Show(this ToastNotification toast)
        {
            Icon icon;
            if (toast.AppLogoOverride != null)
            {
                // For some reasons, this must be done on the same Thread.
                var imageStream = await toast.AppLogoOverride.GetStreamAsync().ConfigureAwait(true);
                var bitmap = (Bitmap)Image.FromStream(imageStream);
                icon = Icon.FromHandle(bitmap.GetHicon());
            }
            else
            {
                icon = GetProcessIcon();
            }

            var tcs = new TaskCompletionSource<object>();

            await semaphore.WaitAsync();

            var notifyManager = new NotificationManagerWindow(CallbackMessage);

            var notifyData = new NotificationIconData();
            notifyData.hWnd = notifyManager.Handle;
            notifyData.cbSize = (uint)Marshal.SizeOf(notifyData);
            notifyData.uCallbackMessage = CallbackMessage;
            notifyData.DUMMYUNIONNAME_uTimeout_uVersion = 0x4;
            notifyData.hIcon = GetProcessIcon().Handle;
            notifyData.szTip = string.Empty;
            notifyData.uFlags = NotificationIconFlags.Icon | NotificationIconFlags.Tip | NotificationIconFlags.Guid | NotificationIconFlags.Message;
            notifyData.guidItem = id;

            if (!notifyData.AddIcon())
            {
                throw new InvalidOperationException("Cannot add icon.");
            }

            notifyData.uFlags = NotificationIconFlags.Info | NotificationIconFlags.Icon | NotificationIconFlags.Guid;
            notifyData.szInfoTitle = toast.Title;
            notifyData.szInfo = toast.Message;
            notifyData.dwInfoFlags = NotificationIconInfoFlags.User | NotificationIconInfoFlags.LargeIcon;
            notifyData.hBalloonIcon = icon.Handle;

            notifyManager.BalloonHide += (s, a) =>
            {
                notifyData.RemoveIcon();
                tcs.SetResult(null);
            };

            notifyManager.BallonClicked += (s, a) =>
            {
                notifyData.RemoveIcon();
                tcs.SetResult(null);
                toast.RaisePressed(EventArgs.Empty);
            };

            Shell.NotifyIcon(NotificationIconMessage.Modify, notifyData);

            await tcs.Task.ConfigureAwait(true);

            // The Window must be disposed on the same Thread.
            notifyManager.Dispose();

            semaphore.Release();
        }

        private static bool AddIcon(this NotificationIconData notifyData)
        {
            return Shell.NotifyIcon(NotificationIconMessage.Add, notifyData) 
                && Shell.NotifyIcon(NotificationIconMessage.SetVersion, notifyData);
        }

        private static bool RemoveIcon(this NotificationIconData notifyData)
        {
            return Shell.NotifyIcon(NotificationIconMessage.Delete, notifyData);
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
