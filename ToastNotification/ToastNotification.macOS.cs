#if __MACOS__
using AppKit;
using Foundation;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.System;

namespace Uno.Extras
{
    public partial class ToastNotification
    {
        /// <summary>
        /// Shows a native Mac OS notification.
        /// </summary>
        public async Task Show()
        {
            // The deprecated NSUserNotification is the only one available on
            // Xamarin.Mac
            var notification = new NSUserNotification
            {
                Title = Title,
                InformativeText = Message
            };

            if (AppLogoOverride != null)
            {
                // Just get stream for all cases, handling the ms-appx:// stuff is tricky.
                var str = await AppLogoOverride.GetDataUrlAsync().ConfigureAwait(false);
                int i = "data:;base64,".Length;
                str = str.Substring(i);

                var data = new NSData(str, NSDataBase64DecodingOptions.None);
                notification.ContentImage = new AppKit.NSImage(data);
            }

            #region Untested Code
            if (ToastButtons != null)
            {
                var first = ToastButtons.FirstOrDefault();
                if (first != null)
                {
                    notification.HasActionButton = true;
                    notification.ActionButtonTitle = first.Content;
                }
                // This is buggy, doesn't seem to work.
                // https://stackoverflow.com/questions/31447882/property-additionalactions-of-nsusernotification-seems-not-working
                if (ToastButtons.Count() >= 2)
                {
                    notification.AdditionalActions =
                        ToastButtons
                            .Skip(1)
                            .Select(button =>
                            {
                                return NSUserNotificationAction.GetAction(GetAppropriateArgument(button), button.Content);
                            })
                            .ToArray();
                }
                notification.UserInfo = new NSMutableDictionary<NSString, NSString>
                {
                    { new NSString("DefaultArgs"), new NSString("foreground," + Arguments) },
                    { new NSString("ActionButtonArgs"), (first != null) ? new NSString(GetAppropriateArgument(first)) : new NSString(string.Empty) }
                };
            }
            #endregion

            var center = NSUserNotificationCenter.DefaultUserNotificationCenter;
            center.DeliverNotification(notification);
        }

        // MacOS supports only one Action button.
        public int GetButtonLimit() => 1;

        static ToastNotification()
        {
            var instance = NSUserNotificationCenter.DefaultUserNotificationCenter;
            // Should allow showing when application is still in focus, else 
            // the behavior is inconsistent with other platforms (and annoying for 
            // new devs).
            instance.ShouldPresentNotification = (center, notification) => true;

            #region Untested Code
            instance.DidActivateNotification += System_DidActivateNotification;
            #endregion
        }

        #region Untested Code
        private static string GetAppropriateArgument(ToastButton button)
        {
            if (button.ShouldDissmiss)
            {
                return "dismiss,";
            }
            switch (button.ActivationType)
            {
                case ToastActivationType.Background:
                    return "background," + button.Arguments;
                case ToastActivationType.Foreground:
                    return "foreground," + button.Arguments;
                case ToastActivationType.Protocol:
                    return "protocol," + button.Protocol.ToString();
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private static void System_DidActivateNotification(object sender, UNCDidActivateNotificationEventArgs e)
        {
            var notification = e.Notification;
            switch (notification.ActivationType)
            {
                case NSUserNotificationActivationType.ActionButtonClicked:
                    HandleArgument(notification.UserInfo["ActionButtonArgs"] as NSString);
                break;
                case NSUserNotificationActivationType.AdditionalActionClicked:
                    HandleArgument(notification.AdditionalActivationAction.Identifier);
                break;
                case NSUserNotificationActivationType.ContentsClicked:
                    HandleArgument(notification.UserInfo["DefaultArgs"] as NSString);
                break;
            }
        }

        public static void HandleArgument(string arg)
        {
            var commaIndex = arg.IndexOf(',');
            var argType = arg.Substring(0, commaIndex);
            var argContent = arg.Substring(commaIndex + 1);

            switch (argType)
            {
                case "dismiss":
                    break;
                case "background":
                    ActivateBackground(argContent);
                    break;
                case "foreground":
                    ActivateForeground(argContent);
                    break;
                case "protocol":
                    _ = Launcher.LaunchUriAsync(new Uri(argContent));
                    break;
            }
        }

        private static void ActivateForeground(string argument)
        {
            FocusApp();

            var app = Windows.UI.Xaml.Application.Current;

            var toastActivatedEventArgs = Reflection.Construct<ToastNotificationActivatedEventArgs>(argument);
            System.Diagnostics.Debug.WriteLine($"{toastActivatedEventArgs.Argument == null}");
            app.Invoke("OnActivated", new[] { toastActivatedEventArgs });
        }

        private static void ActivateBackground(string argument)
        {
            throw new NotImplementedException("Uno Platform does not support background tasks");
        }

        private static void FocusApp()
        {
            NSRunningApplication.CurrentApplication.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
        }
        #endregion
    }
}
#endif