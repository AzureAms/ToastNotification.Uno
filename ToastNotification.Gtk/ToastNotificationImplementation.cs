using GLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using Thread = System.Threading.Thread;

namespace Uno.Extras
{
    public static class ToastNotificationImplementation
    {
        private static Dictionary<Guid, ToastNotification> notifications = new Dictionary<Guid, ToastNotification>();
        private static Dictionary<ToastNotification, Guid> ids = new Dictionary<ToastNotification, Guid>();

        /// <summary>
        /// Shows a native Gtk notification. For best results, you shows initialize a 
        /// <seealso cref="GLib.Application"/>
        /// with the correct application id. Also, gnome-shell requires a desktop file
        /// whose base name matches the application id for the notification to show.
        /// </summary>
        /// <param name="toast">ToastNotification as created in the native library.</param>
        /// <returns></returns>
        public static async Task Show(this ToastNotification toast)
        {
            var notification = new Notification(toast.Title);
            notification.Body = toast.Message + "\n\n\n\n.";

            string path = null;

            if (toast.AppLogoOverride != null)
            {
                var stream = await Hacks.GetImageStreamAsync(toast.AppLogoOverride).ConfigureAwait(false);
                path = Path.GetTempFileName();
                var fileStream = File.OpenWrite(path);
                stream.CopyTo(fileStream);
                stream.Dispose();
                fileStream.Dispose();

                var fileInfo = FileFactory.NewForPath(path);
                FileIcon icon = new FileIcon(fileInfo);

                notification.Icon = icon;

                icon.Dispose();
            }

            Guid id;

            if (ids.ContainsKey(toast))
            {
                id = ids[toast];
            }
            else
            {
                id = Guid.NewGuid();
                ids.Add(toast, id);
                notifications.Add(id, toast);
            }

            await InvokeAsync(() =>
            {
                if (Application.Default == null)
                {
                    var application = new Application(Windows.ApplicationModel.Package.Current.DisplayName + ".Skia.Gtk", ApplicationFlags.None);
                    application.Register(null);
                }
                Application.Default.SendNotification(id.ToString(), notification);
                notification.Dispose();

                // Gtk notifications do not automatically close, so we can decide the time here.
                var waitTime = toast.ToastDuration == ToastDuration.Short ? 7000 : 25000;
                Thread.Sleep(waitTime);
                Application.Default.WithdrawNotification(id.ToString());
                if (path != null) File.Delete(path);
            });
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
                        Gtk.Application.RunIteration();
                }
            });
        }
    }
}
