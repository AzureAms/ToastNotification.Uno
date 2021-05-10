using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Uno.Extras;

namespace ToastNotification.LogoSourceTests.UWP
{
    [TestClass]
    public class Given_LogoSourceFromAsset
    {
        const string AssetPath = "ms-appx:///Assets/sample.png";
        [TestMethod]
        public async Task When_GetStream()
        {
            LogoSource image = null;
            try
            {
                image = new LogoSource(new Uri(AssetPath));
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
                image = new LogoSource(new Uri(AssetPath));
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
                image = new LogoSource(new Uri(AssetPath));

                // Should be the same thing.
                Assert.IsTrue(image.GetSourceUri().ToString() == AssetPath);
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
                image = new LogoSource(new Uri(AssetPath));

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
                await Task.Delay(2000);

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
