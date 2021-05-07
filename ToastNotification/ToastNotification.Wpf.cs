#if NETCOREAPP3_1
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using UwpNot = Microsoft.Toolkit.Uwp.Notifications;

namespace Uno.Extras
{
    public partial class ToastNotification
    {
        private delegate Task ShowFunc(ToastNotification toast);

        static ToastNotification()
        {
            var helper = new DesktopBridge.Helpers();
            _showImpl = helper.IsRunningAsUwp() ? (ShowFunc)ShowUWP : ShowWin32;
        }

        /// <summary>
        /// Shows the toast notification, using the UWP-style notification if possible.
        /// </summary>
        public async void Show()
        {
            await _showImpl(this);
        }

        private static ShowFunc _showImpl;

        private static async Task ShowUWP(ToastNotification toast)
        {
            var builder = new ToastContentBuilder();
            if (toast.AppLogoOverride != null)
            {
                var uri = await GetUriForImage(toast.AppLogoOverride);
                builder.AddAppLogoOverride(uri);
            }

            // Title and Message are required.
            builder.AddText(toast.Title, hintMaxLines: 1)
                   .AddText(toast.Message);

            builder.SetToastDuration(toast.ToastDuration == ToastDuration.Short ? UwpNot.ToastDuration.Short : UwpNot.ToastDuration.Long);

            builder.Show();
        }

        private static async Task<Uri> GetUriForImage(BitmapImage image)
        {
            if (image.UriSource != null) return image.UriSource;

            // Saves image to Image control, then render it with RenderTargetBitmap.
            var bitmap = new RenderTargetBitmap();
            var imageElement = new Image { Source = image };
            await bitmap.RenderAsync(imageElement);

            var folder = ApplicationData.Current.TemporaryFolder;
            var fileName = Guid.NewGuid().ToString();
            var file = await folder.CreateFileAsync(fileName);

            var pixels = await bitmap.GetPixelsAsync();

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                byte[] bytes = pixels.ToArray();
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight,
                                    (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight,
                                    200, 200, bytes);
                await encoder.FlushAsync();
            }

            return new Uri($"ms-appdata:///temp/{fileName}");
        }

        private static async Task ShowWin32(ToastNotification notification)
        {
            throw new NotImplementedException();
        }
    }
}
#endif
