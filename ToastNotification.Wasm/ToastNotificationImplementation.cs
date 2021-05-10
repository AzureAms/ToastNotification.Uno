using System;
using System.Diagnostics;
using System.Linq;
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
        public static async Task Show(this ToastNotification toast)
        {
            // Mono seems to optimize away unsued functions..
            if (await DummyFalseStuff().ConfigureAwait(false))
            {
                Report(null);
            }

            _permission = _permission ?? await QueryPermissionAsync().ConfigureAwait(false);
            
            if ((bool)_permission)
            {
                WebAssemblyRuntime.InvokeJS($@"
                    var n = new Notification(
                        '{WebAssemblyRuntime.EscapeJs(toast.Title)}',
                        {{
                            body: '{WebAssemblyRuntime.EscapeJs(toast.Message)}',
                            {await SetIconIfOveriddenAsync(toast.AppLogoOverride).ConfigureAwait(false)}
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
            await Task.Delay(1).ConfigureAwait(false);
            return false;
        }

        private static async Task<string> SetIconIfOveriddenAsync(LogoSource image, bool comma = false)
        {
            if (image == null)
            {
                return string.Empty;
            }

            string data = null;

            var uri = image.GetSourceUri();
            if ((uri == null) || (new [] { "ms-appx", "ms-appdata" }.Contains(uri.Scheme)))
            {
                data = await image.GetDataUrlAsync().ConfigureAwait(false);
            }
            else
            {
                data = uri.ToString();
            }

            return $"icon: '{data}'" + (comma ? "," : string.Empty);
        }

        /// <summary>
        /// Queries the permission to send notifications from the user.
        /// </summary>
        /// <returns></returns>
        public static Task<bool> QueryPermissionAsync()
        {
            if (_tcs != null)
            {
                return _tcs.Task;
            }

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
