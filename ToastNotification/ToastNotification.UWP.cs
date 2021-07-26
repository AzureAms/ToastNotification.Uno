#if NETFX_CORE
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media.Imaging;
using UwpNot = Microsoft.Toolkit.Uwp.Notifications;

namespace Uno.Extras
{
    public partial class ToastNotification
    {
        TaskCompletionSource<object> _tcs;
        /// <summary>
        /// Shows the toast notification.
        /// </summary>
        public async Task Show()
        {
            var builder = new ToastContentBuilder();
            if (AppLogoOverride != null)
            {
                var uri = AppLogoOverride.GetSourceUri() ?? await AppLogoOverride.GetFileUriAsync().ConfigureAwait(false);
                builder.AddAppLogoOverride(uri);
            }

            // Title and Message are required.
            builder.AddText(Title, hintMaxLines: 1)
                   .AddText(Message);

            builder.SetToastDuration(ToastDuration == ToastDuration.Short ? UwpNot.ToastDuration.Short : UwpNot.ToastDuration.Long);

            if (Timestamp != null)
            {
                builder.AddCustomTimeStamp(Timestamp.Value);
            }

            if (ToastButtons != null)
            {
                foreach (var button in ToastButtons)
                {
                    builder.AddButton(Convert(button));
                }
            }

            foreach (var kvp in _genericArguments)
            {
                builder.AddArgument(kvp.Key, kvp.Value);
            }

            _tcs = new TaskCompletionSource<object>();

            builder.Show(t =>
            {
                TypedEventHandler<Windows.UI.Notifications.ToastNotification, ToastDismissedEventArgs> handler = (sender, args) => { };
                handler = (sender, args) =>
                {
                    sender.Dismissed -= handler;
                    _tcs.SetResult(null);
                };
                t.Dismissed += handler;
            });

            await _tcs.Task;
        }

        public static UwpNot.ToastButton Convert(ToastButton button)
        {
            var uwpButton = new UwpNot.ToastButton(button.Content, button.Arguments)
            {
                ActivationType = Convert(button.ActivationType),
                HintActionId = button.HintActionId,
                ImageUri = button.ImageUri,
                TextBoxId = button.TextBoxId
            };

            if (button.Protocol != null)
            {
                uwpButton.SetProtocolActivation(button.Protocol);
            }

            if (button.ShouldDissmiss)
            {
                uwpButton.SetDismissActivation();
            }

            if (button.ActivationType == ToastActivationType.Background)
            {
                uwpButton.SetBackgroundActivation();
            }

            return uwpButton;    
        }

        public static UwpNot.ToastActivationType Convert(ToastActivationType type)
        {
            return (UwpNot.ToastActivationType)type;
        }
    }
}
#endif