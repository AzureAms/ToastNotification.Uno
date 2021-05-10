
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.Extras;

namespace ToastNotification.LogoSourceTests.UWP
{
    [TestClass]
    public class Given_LogoSourceFromStream
    {
        [TestMethod]
        public async Task When_GetStream()
        {
            LogoSource image = null;
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, Helpers.Sample);
                var response = await client.SendAsync(request);
                var directData = await response.Content.ReadAsByteArrayAsync();
                response.Dispose();
                client.Dispose();

                var imageStream = new MemoryStream(directData);
                image = new LogoSource(imageStream);

                var stream = await image.GetStreamAsync();
                var data = stream.ReadToEnd();

                Assert.IsTrue(directData.SequenceEqual(data));

                image.Dispose();
                stream.Dispose();
                
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
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, Helpers.Sample);
                var response = await client.SendAsync(request);
                var directData = await response.Content.ReadAsByteArrayAsync();
                response.Dispose();
                client.Dispose();

                var imageStream = new MemoryStream(directData);
                image = new LogoSource(imageStream);

                var dataUrl = await image.GetDataUrlAsync();

                Assert.IsTrue(dataUrl == Helpers.DataUrl);
                imageStream.Dispose();
            }
            finally
            {
                image?.Dispose();
            }
        }

        [TestMethod]
        public async Task When_GetUri()
        {
            LogoSource image = null;
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, Helpers.Sample);
                var response = await client.SendAsync(request);
                var directData = await response.Content.ReadAsByteArrayAsync();
                response.Dispose();
                client.Dispose();

                var imageStream = new MemoryStream(directData);
                image = new LogoSource(imageStream);

                // Logos created from stream must be null.
                Assert.IsTrue(image.GetSourceUri() == null);
                imageStream.Dispose();
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
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, Helpers.Sample);
                var response = await client.SendAsync(request);
                var directData = await response.Content.ReadAsByteArrayAsync();
                response.Dispose();
                client.Dispose();

                var imageStream = new MemoryStream(directData);
                image = new LogoSource(imageStream);

                var file = await image.GetStorageFileAsync().ConfigureAwait(false);
                var folder = await file.GetParentAsync();
                var name = file.Name;
                var stream = await file.OpenStreamForReadAsync();

                var fileData = stream.ReadToEnd();

                Assert.IsTrue(fileData.SequenceEqual(directData));

                imageStream.Dispose();
                stream.Dispose();
                image.Dispose();
                image = null;

                // Wait for the file to finish deleting...
                await Task.Delay(2000);

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
