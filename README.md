# Uno.Extras.ToastNotification
Basic ToastNotification for the Uno Platform

## Usage  
```C#
using Uno.Extras;

            var notification = new ToastNotification();
            notification.Title = "Toaster";
            notification.Message = "Hello world!";
            notification.AppLogoOverride = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("https://static.wikia.nocookie.net/os-tan/images/8/8a/764227.png"));
            notification.Show();
```

## Notes  
-   Support for GTK is coming soon.  
-   Support for iOS for macOS: Currently unavailable, but pull requests are welcome!  
-   You can extend the functionality of this library to unsupported platforms by creating an extension method `Show(ToastNotification toast)`.  