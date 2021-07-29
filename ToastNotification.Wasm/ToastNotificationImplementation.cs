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
        private const string JsType = "Uno.Extras.ToastNotificationImplementation";

        private static bool? _permission;

        /// <summary>
        /// Tries to get the relevant permissions and show toast notifications.
        /// </summary>
        /// <param name="toast"></param>
        /// <returns></returns>
        public static async Task Show(this ToastNotification toast)
        {
            if (_permission != true)
            {
                _permission = await QueryPermissionAsync().ConfigureAwait(false);
            }

            if ((bool)_permission)
            {
                await WebAssemblyRuntime.InvokeAsync($@"
                    {JsType}.Show(
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

            return $"icon: '{WebAssemblyRuntime.EscapeJs(data)}'" + (comma ? "," : string.Empty);
        }

        /// <summary>
        /// Queries the permission to send notifications from the user.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> QueryPermissionAsync()
        {
            var permissionStatus = await WebAssemblyRuntime.InvokeAsync($"{JsType}.QueryPermissionAsync()");

            switch (permissionStatus)
            {
                case "FeatureNotSupported":
                    Debug.WriteLine("Feature is not supported on this browser");
                    return false;
                case "denied":
                case "default":
                    return false;
                case "granted":
                    return true;
                default:
                    return false;
            }
        }

        public static int GetButtonLimit(this ToastNotification toast)
        {
            return int.Parse(WebAssemblyRuntime.InvokeJS($"{JsType}.GetButtonLimit()"));
        }
    }
}
