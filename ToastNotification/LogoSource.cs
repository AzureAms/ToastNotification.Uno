using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Uno.Extras
{
    /// <summary>
    /// A class which contains a logo, possibly loaded from Streams or Uri sources.
    /// </summary>
    public class LogoSource : IDisposable
    {
        private byte[] _data;
        private Uri _uri = null;
        private StorageFile _file;
        private Stream _stream;
        private bool _ownsStream = false;

        /// <summary>
        /// Constructs a Logo from a .NET Stream.
        /// </summary>
        /// <param name="s">A stream containing a valid image</param>
        public LogoSource(Stream imageStream)
        {
            if (imageStream == null)
            {
                throw new ArgumentNullException("imageStream");
            }
            _stream = imageStream;
        }

        /// <summary>
        /// Constructs a Logo from a WinRT Stream.
        /// </summary>
        /// <param name="imageStream">A stream containing a valid image</param>
        public LogoSource(IRandomAccessStream imageStream)
        {
            if (imageStream == null)
            {
                throw new ArgumentNullException("imageStream");
            }
            _stream = imageStream.AsStreamForRead();
            _ownsStream = true;
        }

        /// <summary>
        /// Constructs a Logo from a Uri.
        /// </summary>
        /// <param name="imageStream">A Uri pointing to a valid image</param>
        public LogoSource(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            _uri = uri;
        }

        /// <summary>
        /// Gets a stream for reading the image.
        /// </summary>
        /// <returns>A stream which contains the image.</returns>
        public async Task<Stream> GetStreamAsync()
        {
            await InitData().ConfigureAwait(false);
            return new MemoryStream(_data);
        }

        /// <summary>
        /// Gets a StorageFile containing the image.
        /// </summary>
        /// <returns>A file containing the image.</returns>
        public async Task<StorageFile> GetStorageFileAsync()
        {
            if (_file != null)
            {
                return _file;
            }

            await InitData().ConfigureAwait(false);

            var folder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("toastIcons", CreationCollisionOption.OpenIfExists);
            var fileName = Guid.NewGuid().ToString();
            _file = await folder.CreateFileAsync(fileName);

            var uwpStream = await _file.OpenStreamForWriteAsync();
            var outputStream = uwpStream.AsOutputStream();
            var stream = outputStream.AsStreamForWrite();

            await stream.WriteAsync(_data, 0, _data.Length).ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false);
            
            stream.Dispose();
            outputStream.Dispose();
            uwpStream.Dispose();
            
            return _file;
        }

        /// <summary>
        /// Gets a data URL for the image.
        /// This method is often useful on Web Browsers.
        /// </summary>
        /// <returns>A data URL, encoded in base64</returns>
        public async Task<string> GetDataUrlAsync()
        {
            await InitData().ConfigureAwait(false);
            return $"data:;base64,{Convert.ToBase64String(_data)}";
        }

        /// <summary>
        /// Gets the source Uri of the image.
        /// </summary>
        /// <returns>The source used to construct the logo, null if constructed from a stream.</returns>
        public Uri GetSourceUri()
        {
            return _uri;
        }

        /// <summary>
        /// Gets the Uri for the temporary file. This Uri is often used for UWP.
        /// </summary>
        public async Task<Uri> GetFileUriAsync()
        {
            await GetStorageFileAsync().ConfigureAwait(false);
            return new Uri($"ms-appdata:///temp/toastIcons/{_file.Name}");
        }

        public void Dispose()
        {
            if (_ownsStream)
            {
                try
                {
                    _stream?.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // The object has been disposed, ignore it.
                }
                _ownsStream = false;
                _stream = null;
            }
            if (_file != null)
            {
                // Attempts to delete the file until it gets destroyed completely.
                void Check(Task task)
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        _file = null;
                    }
                    else _file?.DeleteAsync().AsTask().ContinueWith(Check);
                }
                _file?.DeleteAsync().AsTask().ContinueWith(Check);
            }
        }

        ~LogoSource()
        {
            Dispose();
        }

        private async Task InitData()
        {
            if (_data != null)
            {
                return;
            }

            WebResponse response = null;

            if (_stream == null)
            {
                if (new string[] { "ms-appx", "ms-appdata" }.Contains(_uri.Scheme))
                {
                    var resourceFile = await StorageFile.GetFileFromApplicationUriAsync(_uri);
                    // For some platforms, resources are not stored properly. We do this to get a persistent file.
                    // https://stackoverflow.com/a/64482832/14009285
                    var folder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("toastIcons", CreationCollisionOption.OpenIfExists);
                    var fileName = Guid.NewGuid().ToString();
                    _file = await resourceFile.CopyAsync(folder, fileName);
                    _stream = await _file.OpenStreamForReadAsync();
                    _ownsStream = true;
                }
                else if (new string[] {"http", "https"}.Contains(_uri.Scheme))
                {
                    var request = WebRequest.Create(_uri);
                    response = await request.GetResponseAsync().ConfigureAwait(false);
                    _stream = response.GetResponseStream();
                    _ownsStream = true;
                }
            }

            var list = await ReadToEndAsync(_stream).ConfigureAwait(false);
            _data = list.ToArray();

            // Mono Android does not allow the stream to live after dispose is called.
            response?.Dispose();
        }

        private static async Task<List<byte>> ReadToEndAsync(Stream stream)
        {
            var bytes = new List<byte>(stream.CanSeek ? (int)stream.Length : 0);
            int badCount = 0;
            var buffer = new byte[8192];
            while (badCount < 100)
            {
                var count = await stream.ReadAsync(buffer, 0, buffer.Length);
                for (int i = 0; i < count; ++i)
                {
                    bytes.Add(buffer[i]);
                }
                if (count == 0)
                {
                    ++badCount;
                }
                else
                {
                    badCount = 0;
                }
            }
            return bytes;
        }
    }
}
