using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Uno.Foundation;
using Windows.UI.Xaml.Media.Imaging;

namespace Uno.Extras
{
    public static class ToastNotificationImplementation
    {
        private static readonly string AskNotificationPermission =
$@"
(function askNotificationPermission() {{
    const reportStatus = Module.mono_bind_static_method('[{Assembly.GetExecutingAssembly().GetName().Name}] {typeof(ToastNotificationImplementation).FullName}:Report');
    function handlePermission(permission) {{
        // set the button to shown or hidden, depending on what the user answers
        reportStatus(Notification.permission.toString());
    }}
    
    // Check Notification.requestPermission's syntax
    function checkNotificationPromise() {{
        try {{
            Notification.requestPermission().then();
        }} catch (e) {{
            return false;
        }}

        return true;
    }}

    // Let's check if the browser supports notifications
    if (!('Notification' in window)) {{
        reportStatus('FeatureNotSupported');
    }} else {{
        if (checkNotificationPromise()) {{
            Notification.requestPermission()
                .then((permission) => {{
                    handlePermission(permission);
                }})
        }} else {{
            Notification.requestPermission(function (permission) {{
                handlePermission(permission);
            }});
        }}
    }}
}})();
";

        private static bool? _permission;
        private static TaskCompletionSource<bool> _tcs;

        /// <summary>
        /// Tries to get the relevant permissions and show toast notifications.
        /// </summary>
        /// <param name="toast"></param>
        /// <returns></returns>
        public static async void Show(this ToastNotification toast)
        {
            // Mono seems to optimize away unsued functions..
            if (await DummyFalseStuff()) Report(null);

            _permission = _permission ?? await QueryPermissionAsync();
            
            if ((bool)_permission)
            {
                WebAssemblyRuntime.InvokeJS($@"
                    var n = new Notification(
                        '{WebAssemblyRuntime.EscapeJs(toast.Title)}',
                        {{
                            body: '{WebAssemblyRuntime.EscapeJs(toast.Message)}',
                            {await SetIconIfOveriddenAsync(toast.AppLogoOverride)}
                        }}
                    );
                ");
            }
            else
            {
                // Fall back to simple alert pop-up.
                WebAssemblyRuntime.InvokeJS($@"alert('{WebAssemblyRuntime.EscapeJs(toast.Message)}');");
            }
        }

        private static async Task<bool> DummyFalseStuff()
        {
            await Task.Delay(1);
            return false;
        }

        private static async Task<string> SetIconIfOveriddenAsync(BitmapImage image, bool comma = false)
        {
            if (image == null) return string.Empty;

            if (image.UriSource != null)
            {
                return $"icon: '{WebAssemblyRuntime.EscapeJs(image.UriSource.AbsoluteUri)}'";
            }

            var tokenSource = new CancellationTokenSource();
            dynamic task = null;
            
            // Try to call this function: https://github.com/unoplatform/uno/blob/master/src/Uno.UI/UI/Xaml/Media/Imaging/BitmapImage.wasm.cs#L25
            image.Invoke<BitmapImage, bool>("TryOpenSourceAsync", new object[] { tokenSource.Token, null, null, task });
            
            tokenSource.Dispose();

            // internal partial struct ImageData: https://github.com/unoplatform/uno/blob/src/Uno.UI/UI/Xaml/Media/ImageData.netstd.cs#L10
            dynamic imageData = await task;
            
            var data = Convert.ToBase64String(imageData.Value);

            return $"icon: '{WebAssemblyRuntime.EscapeJs($"data:;base64,{data}")}'" + (comma ? "," : string.Empty);
        }

        /// <summary>
        /// Queries the permission to send notifications from the user.
        /// </summary>
        /// <returns></returns>
        public static Task<bool> QueryPermissionAsync()
        {
            if (_tcs != null) return _tcs.Task;

            _tcs = new TaskCompletionSource<bool>();
            WebAssemblyRuntime.InvokeJS(AskNotificationPermission);
            return _tcs.Task;
        }

        private static void Report(string status)
        {
            switch (status)
            {
                case "FeatureNotSupported":
                    System.Diagnostics.Debug.WriteLine("Feature is not supported on this browser");
                    _tcs.SetResult(false);
                    break;
                case "denied":
                case "default":
                    _tcs.SetResult(false);
                    break;
                case "granted":
                    _tcs.SetResult(true);
                    break;
                default:
                    _tcs.SetResult(false);
                    break;
            }
            _tcs = null;
        }
    }
}
