using Microsoft.Win32;
using Notification.Natives;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Uno.Extras
{
    public static class ToastNotificationImplementation
    {
        private static readonly object dummy = new object();
        private static Guid id = id = Guid.NewGuid();

        /// <summary>
        /// Shows the toast notification using native Win32 APIs.
        /// </summary>
        /// <param name="toast">ToatsNotification as created in the native library.</param>
        public static async Task Show(this ToastNotification toast)
        {
            Icon icon;
            if (toast.AppLogoOverride != null)
            {
                var imageStream = await GetImageStreamAsync(toast.AppLogoOverride).ConfigureAwait(false);
                var bitmap = (Bitmap)Image.FromStream(imageStream);
                icon = Icon.FromHandle(bitmap.GetHicon());
            }
            else
            {
                icon = GetProcessIcon();
            }

            lock (dummy)
            {
                var notifyData = new NotificationIconData();
                notifyData.hWnd = Process.GetCurrentProcess().MainWindowHandle;
                notifyData.cbSize = (uint)Marshal.SizeOf(notifyData);
                notifyData.DUMMYUNIONNAME_uTimeout_uVersion = 0x4;
                notifyData.hIcon = GetProcessIcon().Handle;
                notifyData.szTip = string.Empty;
                notifyData.uFlags = NotificationIconFlags.Icon | NotificationIconFlags.Tip | NotificationIconFlags.Guid;
                notifyData.guidItem = id;

                notifyData.AddIcon();

                notifyData.uFlags = NotificationIconFlags.Info | NotificationIconFlags.Icon | NotificationIconFlags.Guid;
                notifyData.szInfoTitle = toast.Title;
                notifyData.szInfo = toast.Message;
                notifyData.dwInfoFlags = NotificationIconInfoFlags.User | NotificationIconInfoFlags.LargeIcon;
                notifyData.hBalloonIcon = icon.Handle;

                Shell.NotifyIcon(NotificationIconMessage.Modify, notifyData);

                // Extra 1500ms for the notification to fade out:
                Thread.Sleep(GetNotificationDuration() * 1000 + 1500);
                notifyData.RemoveIcon();
            }
        }

        private static void AddIcon(this NotificationIconData notifyData)
        {
            Shell.NotifyIcon(NotificationIconMessage.Add, notifyData);
            Shell.NotifyIcon(NotificationIconMessage.SetVersion, notifyData);
        }

        private static void RemoveIcon(this NotificationIconData notifyData)
        {
            Shell.NotifyIcon(NotificationIconMessage.Delete, notifyData);
        }

        /// <summary>
        /// Gets the duration of notifications in seconds.
        /// </summary>
        /// <returns>The duration of notification, in seconds.</returns>
        private static int GetNotificationDuration()
        {
            return Registry.CurrentUser.OpenSubKey("Control Panel")
                                      ?.OpenSubKey("Accessibility")
                                      ?.GetValue("MessageDuration") as int?
                                      ?? 9;
        }

        private static Icon GetProcessIcon()
        {
            Icon icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
            return icon;
        }

        private static async Task<Stream> GetImageStreamAsync(BitmapImage image)
        {
            if (image.UriSource != null)
            {
                var uriSource = image.UriSource;
                if (uriSource.Scheme == "ms-appx")
                {
                    var path = uriSource.PathAndQuery;
                    var filePath = GetScaledPath(path);
                    var fileStream = File.OpenRead(filePath);
                    return fileStream;
                }
                else if (uriSource.Scheme == "http" || uriSource.Scheme == "https")
                {
                    var tokenSource = new CancellationTokenSource();
                    var client = new HttpClient();
                    var response = await client.GetAsync(uriSource, HttpCompletionOption.ResponseContentRead, tokenSource.Token);
                    var imageStream = await response.Content.ReadAsStreamAsync();
                    tokenSource.Dispose();
                    return imageStream;
                }
            }
            return image.GetValue<BitmapSource, IRandomAccessStream>("_stream")?.AsStreamForRead();
        }

        private static readonly int[] KnownScales =
        {
            (int)ResolutionScale.Scale100Percent,
            (int)ResolutionScale.Scale120Percent,
            (int)ResolutionScale.Scale125Percent,
            (int)ResolutionScale.Scale140Percent,
            (int)ResolutionScale.Scale150Percent,
            (int)ResolutionScale.Scale160Percent,
            (int)ResolutionScale.Scale175Percent,
            (int)ResolutionScale.Scale180Percent,
            (int)ResolutionScale.Scale200Percent,
            (int)ResolutionScale.Scale225Percent,
            (int)ResolutionScale.Scale250Percent,
            (int)ResolutionScale.Scale300Percent,
            (int)ResolutionScale.Scale350Percent,
            (int)ResolutionScale.Scale400Percent,
            (int)ResolutionScale.Scale450Percent,
            (int)ResolutionScale.Scale500Percent
        };

        private static string GetScaledPath(string rawPath)
        {
            var originalLocalPath =
                Path.Combine(Windows.Application­Model.Package.Current.Installed­Location.Path,
                     rawPath.TrimStart('/').Replace('/', global::System.IO.Path.DirectorySeparatorChar)
                );

            var resolutionScale = (int)DisplayInformation.GetForCurrentView().ResolutionScale;

            var baseDirectory = Path.GetDirectoryName(originalLocalPath);
            var baseFileName = Path.GetFileNameWithoutExtension(originalLocalPath);
            var baseExtension = Path.GetExtension(originalLocalPath);

            for (var i = KnownScales.Length - 1; i >= 0; i--)
            {
                var probeScale = KnownScales[i];

                if (resolutionScale >= probeScale)
                {
                    var filePath = Path.Combine(baseDirectory, $"{baseFileName}.scale-{probeScale}{baseExtension}");

                    if (File.Exists(filePath))
                    {
                        return filePath;
                    }
                }
            }

            return originalLocalPath;
        }
    }
}
