using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace W8lessLabs.Blazor.FileUpload
{
    public class SelectedFileReader : IDisposable
    {
        private HttpClient _http;
        private string _fileBlobUrl;
        private CustomBlobFetch _customBlobFetch;
        private Func<SelectedFile, Task<string>> _fileCreator;
        private Func<string, Task> _fileDestructor;

        private bool _disposed;

        public SelectedFileReader(HttpClient http,
            SelectedFile file, 
            Func<SelectedFile, Task<string>> fileCreator, 
            Func<string, Task> fileDestructor)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            File = file ?? throw new ArgumentNullException(nameof(file));
            _fileCreator = fileCreator ?? throw new ArgumentNullException(nameof(fileCreator));
            _fileDestructor = fileDestructor ?? throw new ArgumentNullException(nameof(fileDestructor));
        }

        public SelectedFile File { get; private set; }

        public async Task<byte[]> GetFileBytesAsync()
        {
            await _PrepareCustomBlobFetchAsync();
            return await _http.GetByteArrayAsync(_customBlobFetch.CustomBlobUrl);
        }

        public async Task<Stream> GetFileStreamAsync()
        {
            await _PrepareCustomBlobFetchAsync();
            return await _http.GetStreamAsync(_customBlobFetch.CustomBlobUrl);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                if (_customBlobFetch != null)
                    _customBlobFetch.Dispose();

                if (_fileBlobUrl != null)
                    _fileDestructor(_fileBlobUrl);
            }
        }

        private async Task _PrepareCustomBlobFetchAsync()
        {
            if (_fileBlobUrl is null)
            {
                _fileBlobUrl = await _fileCreator(File);
                if (_fileBlobUrl == null)
                    throw new NullReferenceException("Unable to create blob URL for file: " + File.Name);
                _customBlobFetch = new CustomBlobFetch(_fileBlobUrl);
            }
        }
    }
}
