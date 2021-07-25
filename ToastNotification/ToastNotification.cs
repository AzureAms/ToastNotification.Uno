using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// Application logo. If not set, on some platforms this will be a generic image.
        /// </summary>
        public LogoSource AppLogoOverride { get; set; }
        /// <summary>
        /// Interval between popup and hiding of toast.
        /// </summary>
        public ToastDuration ToastDuration { get; set; } = ToastDuration.Short;
        /// <summary>
        /// Toast Actions, that can be activated through buttons on the Notification
        /// </summary>
        public IEnumerable<ToastAction> ToastActions { get; set; }
        /// <summary>
        /// Raised when this ToastNotification is pressed.
        /// </summary>
        public event EventHandler Pressed;
        #endregion
        
        internal void RaisePressed(EventArgs args)
        {
            Pressed?.Invoke(this, args);
        }
    }
}
