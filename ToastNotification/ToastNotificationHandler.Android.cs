#if __ANDROID__
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.UI;
using Windows.ApplicationModel.Activation;
using Windows.System;

namespace Uno.Extras
{
    [Service(Name = "com.AzureAms.ToastNotificationService")]
    class ToastNotificationHandler : Service
    {
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            var notificationManager = GetSystemService(NotificationService) as NotificationManager;

            var id = intent.GetIntExtra(ToastNotification.NotificationIdProperty, 0);
            notificationManager.Cancel(id);

            var arg = intent.GetStringExtra(ToastNotification.NotificationArgumentProperty);
            var commaIndex = arg.IndexOf(',');
            var argType = arg.Substring(0, commaIndex);
            var argContent = arg.Substring(commaIndex + 1);

            switch (argType)
            {
                case "dismiss":
                    break;
                case "background":
                    ToastNotification.ActivateBackground(argContent);
                    break;
                case "foreground":
                    ToastNotification.ActivateForeground(argContent, FocusApp);
                    break;
                case "protocol":
                    _ = Launcher.LaunchUriAsync(new Uri(argContent));
                    break;
            }
            
            return StartCommandResult.NotSticky;
        }
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        public override void OnDestroy()
        {
            System.Diagnostics.Debug.WriteLine("Service destroyed.");
        }

        private void FocusApp()
        {
            var intent = new Intent(this, ContextHelper.Current.GetType());
            intent.SetAction(Intent.ActionMain);
            intent.AddCategory(Intent.CategoryLauncher);
            intent.AddFlags(ActivityFlags.NewTask);

            StartActivity(intent);
        }
    }
}
#endif