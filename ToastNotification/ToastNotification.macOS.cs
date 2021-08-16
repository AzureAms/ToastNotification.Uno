#if __MACOS__
using AppKit;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UserNotifications;
using Windows.ApplicationModel.Activation;
using Windows.System;

namespace Uno.Extras
{
    public partial class ToastNotification
    {
        // To-Do: Research on how long these categories must live.
        // Keeping a list of all categories for all notifications shown
        // may lead to serious resource leaks.
        private static List<UNNotificationCategory> _categories = new List<UNNotificationCategory>();
        private static TaskCompletionSource<bool> _tcs;
        private static bool? _permission;
        private static bool? _legacyBehavior;
        private Guid? id;

        public async Task Show()
        {
            _legacyBehavior = _legacyBehavior ?? CheckNewApiVersion();
            if ((bool)_legacyBehavior)
            {
                await ShowLegacy();
                return;
            }

            #region Sharable With iOS
            #region Untested Code
            _permission = _permission ?? await QueryPermissionAsync().ConfigureAwait(false);

            if ((bool)_permission)
            {
                var content = new UNMutableNotificationContent()
                {
                    Title = Title,
                    Subtitle = "",
                    Body = Message
                };

                if (AppLogoOverride != null)
                {
                    var stream = await AppLogoOverride.GetStreamAsync();
                    // We don't need to free this file.
                    // This file will be automatically moved by the System
                    // to some Attachment Data Folder.
                    var tempPath = await CreateTempFileAsync(stream);
                    stream.Dispose();
                    content.Attachments = new[]
                    {
                        UNNotificationAttachment.FromIdentifier(
                            string.Empty, 
                            new NSUrl(tempPath, false),
                            (NSDictionary)null, 
                            out var error)
                    };
                }

                content.UserInfo = new NSMutableDictionary<NSString, NSString>
                {
                    { new NSString("DefaultArgs"), new NSString("foreground," + Arguments) }
                };

                if (ToastButtons != null)
                {
                    var categoryId = Guid.NewGuid().ToString();
                    var currentCategory = UNNotificationCategory.FromIdentifier(
                        categoryId,
                        ToastButtons.Select(button =>
                        {
                            return UNNotificationAction.FromIdentifier(
                                GetAppropriateArgument(button),
                                button.Content,
                                GetAppropriateActionOption(button)
                                );
                        }).ToArray(),
                        new string[] { },
                        UNNotificationCategoryOptions.None
                        );
                    lock (_categories)
                    {
                        _categories.Add(currentCategory);
                        UNUserNotificationCenter.Current.SetNotificationCategories(
                            new NSSet<UNNotificationCategory>(_categories.ToArray()));
                    }
                    content.CategoryIdentifier = categoryId;
                }

                // Create a time-based trigger, interval is in seconds and must be greater than 0.
                // This seems to be a bit safer than the Calendar-based approach,
                // as DateTime.Now might have passed when the notification is shown.
                var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.25, false);

                // Generate a new id for the notification.
                id = id ?? Guid.NewGuid();

                var request = UNNotificationRequest.FromIdentifier(id.ToString(), content, trigger);

                var completionSource = new TaskCompletionSource<object>();

                UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) =>
                {
                    if (err != null)
                    {
                        completionSource.SetException(new InvalidOperationException($"Failed to schedule notification: {err}"));
                    }
                    completionSource.SetResult(null);
                });

                await completionSource.Task.ConfigureAwait(false);
            }
        }

        public static Task<bool> QueryPermissionAsync()
        {
            if (_tcs != null)
            {
                return _tcs.Task;
            }

            lock (_tcs)
            {
                _tcs = new TaskCompletionSource<bool>();
            }

            // request the permission to use local notifications
            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) =>
            {
                _tcs.SetResult(approved);
            });

            return _tcs.Task;
        }

        private static async Task<string> CreateTempFileAsync(Stream stream)
        {
            var fileName = Path.GetTempFileName();
            var fileStream = File.OpenWrite(fileName);
            await stream.CopyToAsync(fileStream);
            fileStream.Dispose();
            return fileName;
        }

        private static UNNotificationActionOptions GetAppropriateActionOption(ToastButton button)
        {
            if (button.ActivationType == ToastActivationType.Foreground && 
                !button.ShouldDissmiss)
            {
                return UNNotificationActionOptions.Foreground;
            }

            return UNNotificationActionOptions.None;
        }
        #endregion
        #endregion

        // MacOS has only one Action Button, but offers a drop-down menu
        // with more options.
        public int GetButtonLimit() => -1;

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

            #region Sharable with iOS
            #region Untested Code
            var currentCenter = UNUserNotificationCenter.Current;
            currentCenter.Delegate = new NotificationCenterDelegates();
            #endregion
            #endregion
        }

        #region Legacy Implementation
        /// <summary>
        /// Shows a native Mac OS notification
        /// using a deprecated implementation.
        /// </summary>
        public async Task ShowLegacy()
        {
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
            else
            {
                notification.UserInfo = new NSMutableDictionary<NSString, NSString>
                {
                    { new NSString("DefaultArgs"), new NSString("foreground," + Arguments) }
                };
            }
            #endregion

            var center = NSUserNotificationCenter.DefaultUserNotificationCenter;
            center.DeliverNotification(notification);
        }

        #region Untested Code
        private static void System_DidActivateNotification(object sender, UNCDidActivateNotificationEventArgs e)
        {
            HandleNotification(e.Notification);
        }

        /// <summary>
        /// Triggers the notification handler. This method is purposedly left public so that the user
        /// can handle notifications when the application starts.
        /// See <see cref="https://developer.apple.com/documentation/foundation/nsusernotificationcenterdelegate/1418378-usernotificationcenter"/>
        /// for more details.
        /// </summary>
        public static void HandleNotification(NSUserNotification notification)
        {   
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
        #endregion
        #endregion

        private static bool CheckNewApiVersion()
        {
            var info = new NSProcessInfo();
            return !info.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(10, 14, 0));
        }

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

        private static void HandleArgument(string arg)
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
            FocusAppAsync().ContinueWith((task) =>
            {
                var app = Windows.UI.Xaml.Application.Current;

                var toastActivatedEventArgs = Reflection.Construct<ToastNotificationActivatedEventArgs>(argument);
                System.Diagnostics.Debug.WriteLine($"{toastActivatedEventArgs.Argument == null}");
                app.Invoke("OnActivated", new[] { toastActivatedEventArgs });
            });
        }

        private static void ActivateBackground(string argument)
        {
            throw new NotImplementedException("Uno Platform does not support background tasks");
        }

        private static async Task FocusAppAsync()
        {
            // New UserNotifications API should automatically focus our application.
            if (!(bool)_legacyBehavior)
            {
                return;
            }
            var currentApp = NSRunningApplication.CurrentApplication;
            currentApp.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);

            while (!currentApp.Active)
            {
                System.Diagnostics.Debug.WriteLine("Waiting for current window to activate...");
                // To avoid a UI deadlock, sleep for a while to allow 
                // other events to proceed.
                await Task.Delay(100).ConfigureAwait(true);
            }
        }

        #region Sharable With iOS
        #region Untested Code
        private class NotificationCenterDelegates : UNUserNotificationCenterDelegate
        {
            public override void DidReceiveNotificationResponse(
                UNUserNotificationCenter center,
                UNNotificationResponse response, 
                Action completionHandler)
            {
                if (response.IsDismissAction)
                {
                    return;
                }
                else if (response.IsDefaultAction)
                {
                    var content = response.Notification.Request.Content;
                    var obj = content.UserInfo["DefaultArgs"];
                    if (obj == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Warning: Default argument is null.");
                    }
                    var argument = obj.ToString();
                    HandleArgument(argument);
                }
                else if (response.IsCustomAction)
                {
                    HandleArgument(response.ActionIdentifier);
                }
                completionHandler();
            }

            public override void WillPresentNotification(
                UNUserNotificationCenter center, 
                UNNotification notification, 
                Action<UNNotificationPresentationOptions> completionHandler)
            {
                completionHandler(UNNotificationPresentationOptions.Banner);
            }
        }
        #endregion
        #endregion
    }
}
#endif