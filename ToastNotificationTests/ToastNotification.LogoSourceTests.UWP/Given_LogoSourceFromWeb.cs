using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Uno.Extras;
using Windows.UI.Xaml.Media.Imaging;

namespace ToastNotification.LogoSourceTests.UWP
{
    [TestClass]
    public class Given_LogoSourceFromWeb
    {
        [TestMethod]
        public async Task When_GetStream()
        {
            LogoSource image = null;
            try
            {
                image = new LogoSource(new Uri(Helpers.Sample));
                var stream = await image.GetStreamAsync();
                var data = stream.ReadToEnd();
                stream.Dispose();

                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, Helpers.Sample);
                var response = await client.SendAsync(request);
                var directData = await response.Content.ReadAsByteArrayAsync();

                response.Dispose();
                client.Dispose();

                Assert.IsTrue(directData.SequenceEqual(data));
            }
            finally
            {
                image?.Dispose();
            }
        }

        [TestMethod]
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

        [TestMethod]
        public void When_GetUri()
        {
            LogoSource image = null;
            try
            {
                image = new LogoSource(new Uri(Helpers.Sample));

                // Should be the same thing.
                Assert.IsTrue(image.GetSourceUri().ToString() == Helpers.Sample);
            }
            finally
            {
                image?.Dispose();
            }
        }

        [TestMethod]
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

                System.Diagnostics.Debug.WriteLine(file.Name);
                System.Diagnostics.Debug.WriteLine(folder.Name);
                Assert.IsTrue(fileData == Helpers.DataUrl);

                stream.Dispose();
                image.Dispose();

                // Wait for the file to delete.
                await Task.Delay(2000).ConfigureAwait(false);

                image = null;
                try
                {
                    var file2 = await folder.GetItemAsync(name);
                    Assert.IsNull(file2);
                }
                catch (FileNotFoundException)
                {
                    // This is expected.
                    Assert.IsTrue(true);
                }
            }
            finally
            {
                image?.Dispose();
            }
        }
    }
}
