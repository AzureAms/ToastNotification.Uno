#if __IOS__
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UserNotifications;
using Windows.System;

namespace Uno.Extras
{
    public partial class ToastNotification
    {
        // To-Do: Research on how long these categories must live.
        // Keeping a list of all categories for all notifications shown
        // may lead to serious resource leaks.
        private static List<UNNotificationCategory> _categories = new List<UNNotificationCategory>();
        private static bool? _permission;
        private Guid? id;

        public async Task Show()
        {
            _permission = _permission ?? await QueryPermissionAsync();

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
                    // The file must be an image file, giving a random ".tmp" file
                    // makes the system throw an error.
                    var tempPath = await CreateTempFileAsync(stream, ".png");
                    stream.Dispose();

                    content.Attachments = new[]
                    {
                        UNNotificationAttachment.FromIdentifier(
                            string.Empty,
                            new NSUrl(tempPath, false),
                            (NSDictionary)null,
                            out var _)
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

                await UNUserNotificationCenter.Current.AddNotificationRequestAsync(request);
            }
        }

        public static async Task<bool> QueryPermissionAsync()
        {
            // request the permission to use local notifications
            var (approved, err) = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(UNAuthorizationOptions.Alert);

            System.Diagnostics.Debug.WriteLine($"[Notifications]: Permission status: {err}");

            return approved;
        }

        private static async Task<string> CreateTempFileAsync(Stream stream, string extension)
        {
            var fileName = Path.GetTempFileName();
            File.Delete(fileName);
            fileName = Path.ChangeExtension(fileName, extension);
            
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

        // iOS opens a menu with a lot of options.
        public int GetButtonLimit() => -1;

        static ToastNotification()
        {
            var currentCenter = UNUserNotificationCenter.Current;
            currentCenter.Delegate = new NotificationCenterDelegates();
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
                    ActivateForeground(argContent, FocusApp);
                    break;
                case "protocol":
                    _ = Launcher.LaunchUriAsync(new Uri(argContent));
                    break;
            }
        }

        private static void FocusApp()
        {
            // There is no need to focus our app:
            // UNNotificationActionOptions.Foreground
            // has already told the system to do our job.
        }

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
                    var argument = obj?.ToString();
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
                completionHandler(UNNotificationPresentationOptions.Alert);
            }
        }
    }
}
#endif