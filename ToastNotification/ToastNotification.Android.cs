#if __ANDROID__
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.App;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Uno.UI;
using Windows.UI.Xaml.Media;

namespace Uno.Extras
{
    public partial class ToastNotification
    {
        #region Private fields
        private const string channelId = "toast_notifications";
        private TaskCompletionSource<NotificationManager> _tcs;
        private Activity _activity;
        private Application _application;
        private NotificationManager _manager;
        private static int notificationId;

        private static int totalId = -1;
        #endregion

        public ToastNotification()
        {
            _activity = ContextHelper.Current as Activity;
            if (_activity == null)
            {
                throw new InvalidOperationException("Cannot send ToastNotifications without a valid Windows.UI.Xaml.ApplicationActivity.");
            }
            _application = (Application)_activity.ApplicationContext;

            notificationId = Interlocked.Increment(ref totalId);
        }

        /// <summary>
        /// Shows the toast notification.
        /// </summary>
        public async void Show()
        {
            if (_manager == null)
            {
                _manager = await GetNotificationManager();
                // Something bad happend.
                if (_manager == null)
                {
                    throw new InvalidOperationException("Failed to get toast notification manager for unknown reasons.");
                }
                CreateNotificationChannel();
            }

            var iconId = _application.ApplicationInfo.Icon;
            if (iconId == 0)
            {
                iconId = Android.Resource.Drawable.SymDefAppIcon;
            }

            NotificationCompat.Builder builder = new NotificationCompat.Builder(_activity, channelId)
                .SetContentTitle(Title)
                .SetContentText(Message)
                .SetSmallIcon(iconId);

            if (AppLogoOverride != null)
            {
                var source = new CancellationTokenSource();
                // This might change...
                // https://github.com/unoplatform/uno/blob/master/src/Uno.UI/UI/Xaml/Media/ImageSource.Android.cs#L168
                var bitmap = await AppLogoOverride.Invoke<ImageSource, Task<Bitmap>>("Open", new object[] { source.Token, null, null, null });
                builder.SetLargeIcon(bitmap);
                source.Dispose();
            }

            var notification = builder.Build();

            _manager.Notify(notificationId, notification);
        }

        private Task<NotificationManager> GetNotificationManager()
        {
            _tcs = new TaskCompletionSource<NotificationManager>();

            try
            {
                if (_activity.GetSystemService(Context.NotificationService) is NotificationManager result)
                {
                    _tcs.SetResult(result);
                }
                return _tcs.Task;
            }
            catch (Java.Lang.Exception)
            {
                // Silence it here, retry.
            }

            _activity.RegisterActivityLifecycleCallbacks(new Callbacks(this));

            try
            {
                if (_activity.GetSystemService(Context.NotificationService) is NotificationManager result)
                {
                    _tcs.SetResult(result);
                }
            }
            catch (Java.Lang.Exception)
            {
                // Silence it here.
            }

            return _tcs.Task;
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channelName = "General notifications";
            var channelDescription = "Toast notifications from application.";
            var channel = new NotificationChannel(channelId, channelName, NotificationImportance.Default)
            {
                Description = channelDescription
            };

            _manager.CreateNotificationChannel(channel);
        }

        #region IActivityLifecycleCallbacks
        private class Callbacks : Java.Lang.Object, Application.IActivityLifecycleCallbacks
        {
            private ToastNotification _owner;

            public Callbacks(ToastNotification owner)
            {
                _owner = owner;
            }

            public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
            {
                // Task completion source might be set before, on the second attempt.
                _owner?._tcs?.TrySetResult(activity.GetSystemService(Context.NotificationService) as NotificationManager);
                _owner = null;
            }

            public void OnActivityDestroyed(Activity activity)
            {
                // Not interested
            }

            public void OnActivityPaused(Activity activity)
            {
                // Not interested
            }

            public void OnActivityResumed(Activity activity)
            {
                // Not interested
            }

            public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
            {
                // Not interested
            }

            public void OnActivityStarted(Activity activity)
            {
                // Not interested
            }

            public void OnActivityStopped(Activity activity)
            {
                // Not interested
            }
        }

        #endregion
    }
}
#endif