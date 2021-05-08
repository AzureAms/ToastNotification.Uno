# Uno.Extras.ToastNotification
Basic ToastNotification for the Uno Platform  
![BuildStatus](https://github.com/trungnt2910/ToastNotification.Uno/actions/workflows/ci.yml/badge.svg)  
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/47592730202a49079eb0875f6df46a95)](https://www.codacy.com/gh/AzureAms/ToastNotification.Uno/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=AzureAms/ToastNotification.Uno&amp;utm_campaign=Badge_Grade)
| Package | Version |
| ---- | ---- |
| Cross platform | [![CrossPlatformShield](https://shields.io/nuget/vpre/ToastNotification.Uno)](https://www.nuget.org/packages/ToastNotification.Uno) |
| WPF | [![WpfShield](https://shields.io/nuget/vpre/ToastNotification.Uno.Wpf)](https://www.nuget.org/packages/ToastNotification.Uno.Wpf) |
| WASM | [![WasmShield](https://shields.io/nuget/vpre/ToastNotification.Uno.Wasm)](https://www.nuget.org/packages/ToastNotification.Uno.Wasm) |
| GTK | [![WasmShield](https://shields.io/nuget/vpre/ToastNotification.Uno.Gtk)](https://www.nuget.org/packages/ToastNotification.Uno.Gtk) |

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