using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Media.Imaging;

namespace Uno.Extras
{
    public partial class ToastNotification
    {
        #region Properties
        /// <summary>
        /// Notification Title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Notification Message
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Arguments, passes to the App when Launched.
        /// </summary>
        public string Arguments => SerializeArgumentsHelper(_genericArguments);
        /// <summary>
        /// Application logo. If not set, on some platforms this will be a generic image.
        /// </summary>
        public LogoSource AppLogoOverride { get; set; }
        /// <summary>
        /// Interval between popup and hiding of toast.
        /// </summary>
        public ToastDuration ToastDuration { get; set; } = ToastDuration.Short;
        /// <summary>
        /// Timestamp of this notification, will be displayed on supported platforms.
        /// </summary>
        public DateTime? Timestamp { get; set; }
        /// <summary>
        /// Buttons on the notification.
        /// </summary>
        public IEnumerable<ToastButton> ToastButtons { get; set; }
        /// <summary>
        /// Raised when this ToastNotification is pressed.
        /// </summary>
        public event EventHandler Pressed;
        #endregion

        public ToastNotification AddButton(ToastButton button)
        {
            if (ToastButtons != _buttons)
            {
                _buttons = new List<ToastButton>();
                ToastButtons = _buttons;
            }
            _buttons.Add(button);
            return this;
        }

        public ToastNotification AddText(string text)
        {
            if (!_titleSet)
            {
                Title = text;
                _titleSet = true;
            }
            else
            {
                Message = Message + text + Environment.NewLine;
            }
            return this;
        }

        public ToastNotification AddCustomTimeStamp(DateTime time)
        {
            Timestamp = time;
            return this;
        }

        public ToastNotification AddAppLogoOverride(LogoSource source)
        {
            AppLogoOverride = source;
            return this;
        }

        public ToastNotification AddAppLogoOverride(Uri uri)
        {
            AppLogoOverride = new LogoSource(uri);
            return this;
        }

        internal static string GetAppropriateArgument(ToastButton button)
        {
            if (button.ShouldDissmiss)
            {
                return "dismiss,";
            }
            switch (button.ActivationType)
            {
                case ToastActivationType.Background:
                    return "background," + button.Arguments;
                case ToastActivationType.Foreground:
                    return "foreground," + button.Arguments;
                case ToastActivationType.Protocol:
                    return "protocol," + button.Protocol.ToString();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // The region below uses Reflection to manually invoke the application 
        // lifecycle callbacks for Uno targets. This is not applicable for 
        // UWP and also causes compilation errors for lower targets.
#if !NETFX_CORE
        internal static void ActivateForeground(string argument, Action FocusApp = null)
        {
            FocusApp();

            var app = Windows.UI.Xaml.Application.Current;

            var toastActivatedEventArgs = Reflection.Construct<ToastNotificationActivatedEventArgs>(argument);
            System.Diagnostics.Debug.WriteLine($"{toastActivatedEventArgs.Argument == null}");
            app.Invoke("OnActivated", new[] { toastActivatedEventArgs });
        }
#endif

        internal static void ActivateBackground(string argument)
        {
            throw new NotImplementedException("Uno Platform does not support background tasks");
        }

        #region AlmostCopiedMITLicensedCodeFromWCT
        /// <summary>
        /// Adds a key/value to the activation arguments that will be returned when the toast notification or its buttons are clicked.
        /// </summary>
        /// <param name="key">The key for this value.</param>
        /// <param name="value">The value itself.</param>
        /// <returns>The current instance of <see cref="ToastNotification"/></returns>
#if WINRT
        [Windows.Foundation.Metadata.DefaultOverload]
        [return: System.Runtime.InteropServices.WindowsRuntime.ReturnValueName("toastContentBuilder")]
#endif
        public ToastNotification AddArgument(string key, string value)
        {
            return AddArgumentHelper(key, value);
        }

        /// <summary>
        /// Adds a key/value to the activation arguments that will be returned when the toast notification or its buttons are clicked.
        /// </summary>
        /// <param name="key">The key for this value.</param>
        /// <param name="value">The value itself.</param>
        /// <returns>The current instance of <see cref="ToastNotification"/></returns>
#if WINRT
        [return: System.Runtime.InteropServices.WindowsRuntime.ReturnValueName("toastContentBuilder")]
#endif
        public ToastNotification AddArgument(string key, int value)
        {
            return AddArgumentHelper(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Adds a key/value to the activation arguments that will be returned when the toast notification or its buttons are clicked.
        /// </summary>
        /// <param name="key">The key for this value.</param>
        /// <param name="value">The value itself.</param>
        /// <returns>The current instance of <see cref="ToastNotification"/></returns>
#if WINRT
        [return: System.Runtime.InteropServices.WindowsRuntime.ReturnValueName("toastContentBuilder")]
#endif
        public ToastNotification AddArgument(string key, double value)
        {
            return AddArgumentHelper(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Adds a key/value to the activation arguments that will be returned when the toast notification or its buttons are clicked.
        /// </summary>
        /// <param name="key">The key for this value.</param>
        /// <param name="value">The value itself.</param>
        /// <returns>The current instance of <see cref="ToastNotification"/></returns>
#if WINRT
        [return: System.Runtime.InteropServices.WindowsRuntime.ReturnValueName("toastContentBuilder")]
#endif
        public ToastNotification AddArgument(string key, float value)
        {
            return AddArgumentHelper(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Adds a key/value to the activation arguments that will be returned when the toast notification or its buttons are clicked.
        /// </summary>
        /// <param name="key">The key for this value.</param>
        /// <param name="value">The value itself.</param>
        /// <returns>The current instance of <see cref="ToastNotification"/></returns>
#if WINRT
        [return: System.Runtime.InteropServices.WindowsRuntime.ReturnValueName("toastContentBuilder")]
#endif
        public ToastNotification AddArgument(string key, bool value)
        {
            return AddArgumentHelper(key, value ? "1" : "0"); // Encode as 1 or 0 to save string space
        }

#if !WINRT
        /// <summary>
        /// Adds a key/value to the activation arguments that will be returned when the toast notification or its buttons are clicked.
        /// </summary>
        /// <param name="key">The key for this value.</param>
        /// <param name="value">The value itself. Note that the enums are stored using their numeric value, so be aware that changing your enum number values might break existing activation of toasts currently in Action Center.</param>
        /// <returns>The current instance of <see cref="ToastNotification"/></returns>
        public ToastNotification AddArgument(string key, Enum value)
        {
            return AddArgumentHelper(key, ((int)(object)value).ToString());
        }
#endif

        private ToastNotification AddArgumentHelper(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _genericArguments[key] = value;

            if (ToastButtons != null)
            {
                foreach (var button in ToastButtons)
                {
                    if (button.CanAddArguments())
                    {
                        button.AddArgument(key, value);
                    }
                }
            }

            return this;
        }

        private string SerializeArgumentsHelper(IDictionary<string, string> arguments)
        {
            var args = new ToastArguments();

            foreach (var a in arguments)
            {
                args.Add(a.Key, a.Value);
            }

            return args.ToString();
        }

        private string AddArgumentHelper(string existing, string key, string value)
        {
            string pair = ToastArguments.EncodePair(key, value);

            if (existing == null)
            {
                return pair;
            }
            else
            {
                return existing + ToastArguments.Separator + pair;
            }
        }
        #endregion

        internal List<ToastButton> _buttons = new List<ToastButton>();

        internal void RaisePressed(EventArgs args)
        {
            Pressed?.Invoke(this, args);
        }

        private bool _titleSet;
        private bool _usingCustomArguments;
        private readonly Dictionary<string, string> _genericArguments = new Dictionary<string, string>();
    }
}
