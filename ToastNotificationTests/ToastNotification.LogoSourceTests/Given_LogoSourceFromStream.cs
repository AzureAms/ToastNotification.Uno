using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Uno.Extras;
using Windows.UI.Xaml.Media.Imaging;

namespace ToastNotification.LogoSourceTests
{
    class Given_LogoSourceFromStream
    {
        [Test]
        public async Task When_GetStream()
        {
            LogoSource image = null;
            try
            {
                using var wc = new WebClient();
                byte[] directData = wc.DownloadData(Helpers.Sample);

                using var imageStream = new MemoryStream(directData);
                image = new LogoSource(imageStream);

                using var stream = await image.GetStreamAsync();
                var data = stream.ReadToEnd();

                Assert.IsTrue(directData.SequenceEqual(data));
            }
            finally
            {
                image?.Dispose();
            }
        }

        [Test]
        public async Task When_GetDataUrl()
        {
            LogoSource image = null;
            try
            {
                using var wc = new WebClient();
                byte[] directData = wc.DownloadData(Helpers.Sample);

                using var imageStream = new MemoryStream(directData);
                image = new LogoSource(imageStream);

                var dataUrl = await image.GetDataUrlAsync();

                Assert.IsTrue(dataUrl == Helpers.DataUrl);
            }
            finally
            {
                image?.Dispose();
            }
        }

        [Test]
        public void When_GetUri()
        {
            LogoSource image = null;
            try
            {
                using var wc = new WebClient();
                byte[] directData = wc.DownloadData(Helpers.Sample);

                using var imageStream = new MemoryStream(directData);
                image = new LogoSource(imageStream);

                // Logos created from stream must be null.
                Assert.IsTrue(image.GetSourceUri() == null);
            }
            finally
            {
                image?.Dispose();
            }
        }

        [Test]
        public async Task When_GetStorageFile()
        {
            LogoSource image = null;
            try
            {
                using var wc = new WebClient();
                byte[] directData = wc.DownloadData(Helpers.Sample);

                using var imageStream = new MemoryStream(directData);
                image = new LogoSource(imageStream);

                var file = await image.GetStorageFileAsync().ConfigureAwait(false);
                var folder = await file.GetParentAsync();
                var name = file.Name;
                var stream = await file.OpenStreamForReadAsync();

                var fileData = stream.ReadToEnd();

                Assert.IsTrue(fileData.SequenceEqual(directData));

                stream.Dispose();
                image.Dispose();
                image = null;
                try
                {
                    var file2 = await folder.GetItemAsync(name);
                    Assert.IsNull(file2);
                }
                catch (FileNotFoundException)
                {
                    // This is expected.
                    Assert.Pass();
                }
            }
            finally
            {
                image?.Dispose();
            }
        }
    }
}
