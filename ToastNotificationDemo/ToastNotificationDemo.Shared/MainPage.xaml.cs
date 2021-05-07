using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Extras;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Toaster_Click(object sender, RoutedEventArgs args)
        {
            var notification = new ToastNotification();
            notification.Title = "Toaster";
            notification.Message = "Hello world!";
            notification.AppLogoOverride = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("https://static.wikia.nocookie.net/os-tan/images/8/8a/764227.png"));
            notification.Show();
        }
    }
}
