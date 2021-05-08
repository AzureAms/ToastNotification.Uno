#if NETSTANDARD
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Uno.Extras
{
    internal static class Hacks
    {
        /// <summary>
        /// Gets a stream containing the image data of a BitmapImage using reflection.
        /// </summary>
        /// <param name="image">Source image</param>
        /// <returns></returns>
        public static async Task<Stream> GetImageStreamAsync(BitmapImage image)
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
#endif