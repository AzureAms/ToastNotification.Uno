#if __IOS__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserNotifications;

namespace Uno.Extras
{
    public partial class ToastNotification
    {
        private static TaskCompletionSource<bool> _tcs;
        private static bool? _permission;
        private Guid? id;

        /// <summary>
        /// Shows a native iOS notification.
        /// Code not tested and unsupported.
        /// </summary>
        public async Task Show()
        {
            _permission = _permission ?? await QueryPermissionAsync().ConfigureAwait(false);

            if ((bool)_permission)
            {
                var content = new UNMutableNotificationContent()
                {
                    Title = Title,
                    Subtitle = "",
                    Body = Message,
                    Badge = 1
                };

                // Create a time-based trigger, interval is in seconds and must be greater than 0.
                UNNotificationTrigger trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.25, false);

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

            _tcs = new TaskCompletionSource<bool>();

            // request the permission to use local notifications
            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) =>
            {
                _tcs.SetResult(approved);
            });

            return _tcs.Task;
        }
    }
}
#endif