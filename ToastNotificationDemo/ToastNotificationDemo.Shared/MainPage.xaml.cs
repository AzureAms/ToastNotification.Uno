using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Extras;
#if __WASM__
using Uno.Foundation;
#endif
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ToastNotificationDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string sample = "https://static.wikia.nocookie.net/os-tan/images/8/8a/764227.png";

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void ToasterUrl_Click(object sender, RoutedEventArgs args)
        {
            var notification = new ToastNotification();
            notification.Title = "Toaster with image from Url";
            notification.Message = "Hello world!";
            var logoSource = new LogoSource(new Uri(sample));
            notification.AppLogoOverride = logoSource;
            await notification.Show().ConfigureAwait(false);
            logoSource.Dispose();
        }

        private async void ToasterStream_Click(object sender, RoutedEventArgs args)
        {
#if __WASM__
            var data = await WasmWebClient.DownloadDataTaskAsync(sample);
#else
            var request = WebRequest.CreateHttp(sample);
            var response = request.GetResponse();
            var responseStream = response.GetResponseStream();
            var data = ReadToEnd(responseStream);
            responseStream.Dispose();
            response.Dispose();
#endif

            var notification = new ToastNotification();
            notification.Title = "Toaster with image from stream";
            notification.Message = "Hello world!";
            var logoSource = new LogoSource(new MemoryStream(data));
            notification.AppLogoOverride = logoSource;
            await notification.Show().ConfigureAwait(false);
            logoSource.Dispose();
        }

        private async void ToasterResource_Click(object sender, RoutedEventArgs args)
        {
            var notification = new ToastNotification();
            notification.Title = "Toaster with image from package";
            notification.Message = "Hello world!";
#if __ANDROID__
            var logoSource = new LogoSource(new Uri("ms-appx:///sample.png"));
#else
            var logoSource = new LogoSource(new Uri("ms-appx:///Assets/sample.png"));
#endif
            notification.AppLogoOverride = logoSource;
            await notification.Show().ConfigureAwait(false);
            logoSource.Dispose();
        }

        private static byte[] ReadToEnd(Stream stream)
        {
            var bytes = new List<byte>(stream.CanSeek ? (int)stream.Length : 0);
            var buffer = new byte[8192];
            int badCount = 0;
            while (badCount < 100)
            {
                var count = stream.Read(buffer, 0, 8192);
                for (int i = 0; i < count; ++i) bytes.Add(buffer[i]);
                if (count == 0)
                {
                    ++badCount;
                }
                else
                {
                    badCount = 0;
                }
            }
            return bytes.ToArray();
        }
    }
}
