#if NETFX_CORE
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
        /// <summary>
        /// Shows the toast notification.
        /// </summary>
        public async Task Show()
        {
            var builder = new ToastContentBuilder();
            if (AppLogoOverride != null)
            {
                var uri = await GetUriForImage(AppLogoOverride);
                builder.AddAppLogoOverride(uri);
            }

            // Title and Message are required.
            builder.AddText(Title, hintMaxLines: 1)
                   .AddText(Message);

            builder.SetToastDuration(ToastDuration == ToastDuration.Short ? UwpNot.ToastDuration.Short : UwpNot.ToastDuration.Long);

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
    }
}
#endif