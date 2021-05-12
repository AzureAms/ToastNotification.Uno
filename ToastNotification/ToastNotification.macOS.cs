#if __MACOS__
using Foundation;
using System.Threading.Tasks;

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

            var center = NSUserNotificationCenter.DefaultUserNotificationCenter;
            center.DeliverNotification(notification);
        }

        static ToastNotification()
        {
            var instance = NSUserNotificationCenter.DefaultUserNotificationCenter;
            // Should allow showing when application is still in focus, else 
            // the behavior is inconsistent with other platforms (and annoying for 
            // new devs).
            instance.ShouldPresentNotification = (center, notification) => true;
        }
    }
}
#endif