using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Uno.Foundation;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
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

            var actions = new List<(string type, string argument, string title)>();
            actions.Add(("foreground", toast.Arguments, "default"));

            if (toast.ToastButtons != null)
            {
                actions.AddRange(toast.ToastButtons.Select(button => (button.GetAppropriateType(), button.Arguments, button.Content)));
            }

            if ((bool)_permission)
            {
                await WebAssemblyRuntime.InvokeAsync($@"
                    {JsType}.Show(
                        '{WebAssemblyRuntime.EscapeJs(toast.Title)}',
                        {{
                            body: '{WebAssemblyRuntime.EscapeJs(toast.Message)}',
                            {await SetIconIfOveriddenAsync(toast.AppLogoOverride).ConfigureAwait(false)}
                        }},
                        {actions.Serialize()}
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

        private static async void OnNotificationClick(string argument)
        {
            argument = argument ?? string.Empty;
            // The ServiceWorker has already taken care of focusing,
            // we now only need to activate the relevant function.
            var app = Application.Current;
            // The Javascript might finish initializing before the app.
            while (app == null)
            {
                await Task.Delay(128);
                app = Application.Current;
            }
            try
            {
                var args = Reflection.Construct<ToastNotificationActivatedEventArgs>(argument);
                args.SetProperty("Argument", argument);
                app.InvokeVirtual("OnActivated", new object[] { args });
            }
            catch (Exception e)
            {
                // Should ignore this exception, and write it
                // to the console.
#if DEBUG
                Console.WriteLine(e);
#endif
            }
        }

        private static string GetAppropriateType(this ToastButton button)
        {
            if (button.ShouldDissmiss)
            {
                return "dismiss";
            }
            
            switch (button.ActivationType)
            {
                case ToastActivationType.Foreground:
                    return "foreground";
                case ToastActivationType.Background:
                    return "background";
                case ToastActivationType.Protocol:
                    return "protocol";
                default:
                    throw new ArgumentOutOfRangeException("Invalid value");
            }
        }

        // Don't wanna use Newtonsoft.Json here.
        private static string Serialize(this List<(string type, string argument, string title)> actions)
        {
            return $"[{string.Join(",", actions.Select(action => $"{{type:'{WebAssemblyRuntime.EscapeJs(action.type)}',argument:'{WebAssemblyRuntime.EscapeJs(action.argument)}',title:'{WebAssemblyRuntime.EscapeJs(action.title).Replace("'", "\\'")}'}}"))}]";
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

        public static string GetJsInteropName<TFunc>(TFunc func) where TFunc : Delegate
        {
            var info = func.GetMethodInfo();
            var asmName = info.DeclaringType.Assembly.GetName().Name;
            var typeName = info.DeclaringType.FullName;
            var funcName = info.Name;

            return $"[{asmName}] {typeName}:{funcName}";
        }

        static ToastNotificationImplementation()
        {
            WebAssemblyRuntime.InvokeJS($"{JsType}.SetNotificationClickHandler" +
                $"('{WebAssemblyRuntime.EscapeJs(GetJsInteropName<Action<string>>(OnNotificationClick))}')");
        }
    }
}
