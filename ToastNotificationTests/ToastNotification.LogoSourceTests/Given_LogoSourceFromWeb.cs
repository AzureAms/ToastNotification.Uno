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
    class Given_LogoSourceFromWeb
    {
        [Test]
        public async Task When_GetStream()
        {
            LogoSource image = null;
            try
            {
                image = new LogoSource(new Uri(Helpers.Sample));
                using var stream = await image.GetStreamAsync();
                var data = stream.ReadToEnd();

                using var wc = new WebClient();
                var directData = wc.DownloadData(Helpers.Sample);

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
                image = new LogoSource(new Uri(Helpers.Sample));
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
            var image = new LogoSource(new Uri(Helpers.Sample));

            // Should be the same thing.
            Assert.IsTrue(image.GetSourceUri().ToString() == Helpers.Sample);
        }

        [Test]
        public async Task When_GetStorageFile()
        {
            LogoSource image = null;
            try
            {
                image = new LogoSource(new Uri(Helpers.Sample));

                var file = await image.GetStorageFileAsync().ConfigureAwait(false);
                var folder = await file.GetParentAsync();
                var name = file.Name;
                var stream = await file.OpenStreamForReadAsync();

                var fileData = $"data:;base64,{Convert.ToBase64String(stream.ReadToEnd())}";

                System.Diagnostics.Debugger.Break();
                Assert.IsTrue(fileData == Helpers.DataUrl);

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
