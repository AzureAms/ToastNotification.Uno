# Uno.Extras.ToastNotification
Basic ToastNotification for the Uno Platform.  
Provides basic, signature-compatible APIs with [Microsoft.Toolkit.Uwp.Notifications](https://www.nuget.org/packages/Microsoft.Toolkit.Uwp.Notifications/), for the Uno Platform.    
![BuildStatus](https://github.com/AzureAms/ToastNotification.Uno/actions/workflows/ci.yml/badge.svg)  
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/47592730202a49079eb0875f6df46a95)](https://www.codacy.com/gh/AzureAms/ToastNotification.Uno/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=AzureAms/ToastNotification.Uno&amp;utm_campaign=Badge_Grade)
| Package | Version |
| ---- | ---- |
| Cross platform | [![CrossPlatformShield](https://shields.io/nuget/vpre/ToastNotification.Uno)](https://www.nuget.org/packages/ToastNotification.Uno) |
| WPF | [![WpfShield](https://shields.io/nuget/vpre/ToastNotification.Uno.Wpf)](https://www.nuget.org/packages/ToastNotification.Uno.Wpf) |
| WASM | [![WasmShield](https://shields.io/nuget/vpre/ToastNotification.Uno.Wasm)](https://www.nuget.org/packages/ToastNotification.Uno.Wasm) |
| GTK | [![WasmShield](https://shields.io/nuget/vpre/ToastNotification.Uno.Gtk)](https://www.nuget.org/packages/ToastNotification.Uno.Gtk) |

## Usage  
```C#
#if __ANDROID__
            var logoSource = new LogoSource(new Uri("ms-appx:///sample.png"));
#else
            var logoSource = new LogoSource(new Uri("ms-appx:///Assets/sample.png"));
#endif
            logoSource.Dispose();

            await new ToastNotification()
                .AddAppLogoOverride(logoSource)
                .AddText("Toaster with buttons")
                .AddText("Hello world!")
                .AddText("Wanna play with me?")
                .AddButton(new ToastButton()
                    .SetContent("Let's go!")
                    .AddArgument("ShouldPlay", true)
                    )
                .AddButton(new ToastButton()
                    .SetContent("Another time")
                    .SetDismissActivation()
                    )
                .Show();
```

## Notes  
-   Help is wanted in providing support for Tizen. Pull requests are welcome.
-   Support for macOS is implemented using deprecated APIs. Please help us fix this by fixing [#21](https://github.com/AzureAms/ToastNotification.Uno/issues/21).
-   iOS notifications are implemented, but not tested. Please report any issues.
-   You can extend the functionality of this library to unsupported platforms by creating an extension method `Show(ToastNotification toast)`.  