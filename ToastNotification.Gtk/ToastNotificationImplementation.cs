﻿using GLib;
using Notification.Natives;
using Notifications.DBus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Tmds.DBus;
using Uno.UI.Runtime.Skia;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Task = System.Threading.Tasks.Task;
using Thread = System.Threading.Thread;

namespace Uno.Extras
{
    public static class ToastNotificationImplementation
    {
        private static readonly Dictionary<uint, string> _notificationIds = new Dictionary<uint, string>();
        private static readonly SemaphoreSlim _dictionarySemaphore = new SemaphoreSlim(1);

        private static readonly INotifications _service =
            Connection.Session.CreateProxy<INotifications>("org.freedesktop.Notifications", "/org/freedesktop/Notifications");

        private static readonly Task _registerTask;

        delegate long d_g_get_monotonic_time();
        private static d_g_get_monotonic_time g_get_monotonic_time = GtkFuncLoader.LoadFunction<d_g_get_monotonic_time>("GLib", "g_get_monotonic_time");

        /// <summary>
        /// Shows a notification using DBus, according to the standard here:
        /// https://developer.gnome.org/notification-spec/
        /// </summary>
        /// <param name="toast">ToastNotification as created in the shared library.</param>
        /// <returns></returns>
        public static async Task Show(this ToastNotification toast)
        {
            await _registerTask;

            string path = string.Empty;

            if (toast.AppLogoOverride != null)
            {
                var stream = await toast.AppLogoOverride.GetStreamAsync().ConfigureAwait(false);
                path = Path.GetTempFileName();
                var fileStream = File.OpenWrite(path);
                stream.CopyTo(fileStream);
                stream.Dispose();
                fileStream.Dispose();
            }

            var actions = new List<string>();
            actions.Add("default");
            actions.Add(string.Empty);

            if (toast.ToastButtons != null)
            {
                foreach (var button in toast.ToastButtons)
                {
                    actions.Add(ToastNotification.GetAppropriateArgument(button));
                    actions.Add(button.Content);
                }
            }

            var appName = Assembly.GetEntryAssembly().GetName().Name;
            var duration = toast.ToastDuration == ToastDuration.Short ? 7000 : 25000;

            // This is to prevent id from being deleted before added to the dictionary.
            await _dictionarySemaphore.WaitAsync();
            var id = await _service.NotifyAsync(appName, 0, UrlEncode(path), toast.Title, toast.Message, actions.ToArray(), new Dictionary<string, object>(), duration);
            _notificationIds.Add(id, toast.Arguments);
            _dictionarySemaphore.Release();

            // The file should be deleted, as the service seems to have
            // kept a copy of the image.
            File.Delete(path);
        }

        // On GTK, we don't know how many buttons notifications support.
        public static int GetButtonLimit(this ToastNotification toast) => -1;

        static ToastNotificationImplementation()
        {
            _registerTask = RegisterAsync();
        }

        private static async Task RegisterAsync()
        {
            await _service.WatchActionInvokedAsync(OnActivation);
            await _service.WatchNotificationClosedAsync(OnNotificationClose);
        }

        private static async void OnActivation((uint id, string key) arg)
        {
            await _dictionarySemaphore.WaitAsync();
            if (!_notificationIds.ContainsKey(arg.id))
            {
                goto bail;
            }
            if (arg.key == "default")
            {
                ToastNotification.ActivateForeground(_notificationIds[arg.id], FocusApp);
            }
            else
            {
                var splitIndex = arg.key.IndexOf(",", StringComparison.InvariantCulture);
                if (splitIndex == -1)
                {
                    goto bail;
                }
                var operation = arg.key.Substring(0, splitIndex);
                var data = arg.key.Substring(splitIndex + 1);
                switch (operation)
                {
                    case "dismiss":
                    break;
                    case "protocol":
                        await Launcher.LaunchUriAsync(new Uri(data));
                    break;
                    case "background":
                        ToastNotification.ActivateBackground(data);
                    break;
                    case "foreground":
                        ToastNotification.ActivateForeground(data, FocusApp);
                    break;
                    default:
#if DEBUG
                        Console.WriteLine($"Unknown activation type: {operation}");
#endif
                        ToastNotification.ActivateForeground(data, FocusApp);
                    break;
                }
            }
            // Signals are somehow frequently called twice.
            // This is to ensure stuff are not invoked randomly.
            _notificationIds.Remove(arg.id);
bail:       _dictionarySemaphore.Release();
        }

        private static async void OnNotificationClose((uint id, uint reason) arg)
        {
            await _dictionarySemaphore.WaitAsync();
            _notificationIds.Remove(arg.id);
            _dictionarySemaphore.Release();
        }

        private static void FocusApp()
        {
            var window = GtkHost.Window;

            // A valid timestamp is required, else the 
            // window will not successfully activate.
            window.PresentWithTime(TryGetTimestamp());
        }

        private static Task InvokeAsync(Action a)
        {
            return Task.Run(() =>
            {
                long good = 0;
                Gtk.Application.Invoke((sender, args) =>
                {
                    a();
                    Interlocked.Increment(ref good);
                });
                // Keeps the application responsive.
                while (Interlocked.Read(ref good) != 1)
                {
                    while (Gtk.Application.EventsPending())
                    {
                        Gtk.Application.RunIteration();
                    }
                }
            });
        }

        private static uint TryGetTimestamp()
        {
            if (g_get_monotonic_time != null)
            {
                return (uint)(g_get_monotonic_time() / 1000);
            }
            else
            {
                return 0;
            }
        }

        // Stolen from the Uno Platform
        private static string UrlEncode(string path)
        {
            var uri = new StringBuilder();
            foreach (var ch in path)
            {
                if (('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') || ('0' <= ch && ch <= '9') ||
                    "-._~".Contains(ch))
                {
                    uri.Append(ch);
                }
                else if (ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar)
                {
                    uri.Append('/');
                }
                else
                {
                    var bytes = Encoding.UTF8.GetBytes(new[] { ch });
                    foreach (var b in bytes)
                    {
                        uri.Append($"%{b:X2}");
                    }
                }
            }
            return "file://" + uri;
        }
    }
}
