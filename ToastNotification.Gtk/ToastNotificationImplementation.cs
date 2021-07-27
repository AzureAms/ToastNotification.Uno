using GLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Task = System.Threading.Tasks.Task;
using Thread = System.Threading.Thread;

namespace Uno.Extras
{
    public static class ToastNotificationImplementation
    {
        private static Dictionary<Guid, ToastNotification> notifications = new Dictionary<Guid, ToastNotification>();
        private static Dictionary<ToastNotification, Guid> ids = new Dictionary<ToastNotification, Guid>();

        private const string LaunchFromNotificationForeground = "LaunchFromNotificationForeground";
        private const string LaunchFromNotificationBackground = "LaunchFromNotificationBackground";
        private const string LaunchProtocolFromNotification = "LaunchProtocolFromNotification";
        private const string DismissAction = "DismissNotification";

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
            notification.Body = toast.Message;

            string path = null;

            if (toast.AppLogoOverride != null)
            {
                var stream = await toast.AppLogoOverride.GetStreamAsync().ConfigureAwait(false);
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

            notification.SetDefaultActionAndTargetValue("app." + LaunchFromNotificationForeground, new Variant(toast.Arguments));
            
            if (toast.ToastButtons != null)
            {
                foreach (var button in toast.ToastButtons)
                {
                    notification.AddButtonWithTargetValue(button.Content, button.GetAppropriateAction(),
                        new Variant(button.ActivationType != ToastActivationType.Protocol ? button.Arguments : button.Protocol.ToString()));
                }
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
                
                // Gnome uses this temporary file to display the icon. We must not delete this file
                // before the notification withdraws.
                if (path != null)
                {
                    File.Delete(path);
                }
            }).ConfigureAwait(false);
        }

        // On GTK, we don't know how many buttons notifications support.
        public static int GetButtonLimit(this ToastNotification toast) => -1;

        static ToastNotificationImplementation()
        {
            var foregroundAction = new SimpleAction(LaunchFromNotificationForeground, VariantType.String);
            var backgroundAction = new SimpleAction(LaunchFromNotificationBackground, VariantType.String);
            var protocolAction = new SimpleAction(LaunchProtocolFromNotification, VariantType.String);
            var dismissAction = new SimpleAction(DismissAction, VariantType.Any);

            foregroundAction.Activated += ForegroundAction_Activated;
            backgroundAction.Activated += BackgroundAction_Activated;
            protocolAction.Activated += ProtocolAction_Activated;

            var app = Application.Default;

            app.AddAction(foregroundAction);
            app.AddAction(backgroundAction);
            app.AddAction(protocolAction);
            app.AddAction(dismissAction);
        }

        private static void ForegroundAction_Activated(object o, ActivatedArgs args)
        {
            var arguments = (string)args.P0;
            ActivateApp(arguments);
        }

        private static void BackgroundAction_Activated(object o, ActivatedArgs args)
        {
            var arguments = (string)args.P0;
            ActivateBackground(arguments);
        }

        private static void ProtocolAction_Activated(object o, ActivatedArgs args)
        {
            var protocol = (string)args.P0;
            _ = Launcher.LaunchUriAsync(new Uri(protocol));
        }

        private static void ActivateApp(string argument)
        {
            var app = Windows.UI.Xaml.Application.Current;
            var args = Reflection.Construct<ToastNotificationActivatedEventArgs>(argument);
            app.Invoke("OnActivated", new object[] { args });
        }

        private static void ActivateBackground(string argument)
        {
            throw new NotImplementedException("Uno Platform does not support background tasks");
        }

        private static string GetAppropriateAction(this ToastButton button)
        {
            if (button.ShouldDissmiss)
            {
                return DismissAction;
            }
            switch (button.ActivationType)
            {
                case ToastActivationType.Background:
                    return LaunchFromNotificationBackground;
                case ToastActivationType.Foreground:
                    return LaunchFromNotificationForeground;
                case ToastActivationType.Protocol:
                    return LaunchProtocolFromNotification;
                default:
                    throw new ArgumentOutOfRangeException("ToastActivationType does not exist!");
            }
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
    }
}
