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
        public BitmapImage AppLogoOverride { get; set; }
        /// <summary>
        /// Interval between popup and hiding of toast.
        /// </summary>
        public ToastDuration ToastDuration { get; set; } = ToastDuration.Short;
        #endregion
    }
}
