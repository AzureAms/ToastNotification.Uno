using System;
using System.Threading.Tasks;
using Tizen.Applications.Notifications;

namespace Uno.Extras
{
    public static class ToastNotificationImplementation
    {
        /// <summary>
        /// Shows a native Tizen Quick Panel notification.
        /// </summary>
        /// <param name="toast">ToatsNotification as created in the native library.</param>
        /// <returns></returns>
        public static async Task Show(this ToastNotification toast)
        {
            var notification = new Notification()
            {
                Title = toast.Title,
                Content = toast.Message,
            };

            if (toast.AppLogoOverride != null)
            {
                var file = await toast.AppLogoOverride.GetStorageFileAsync().ConfigureAwait(false);
                notification.Icon = file.Path;
            }

            NotificationManager.Post(notification);
        }
    }
}
