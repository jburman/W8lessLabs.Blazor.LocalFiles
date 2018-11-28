using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Blazor.LocalFiles
{
    public class SelectedFileReader : IDisposable
    {
        private HttpClient _http;
        private CustomBlobFetch _customBlobFetch;
        private FileBlobUrls _fileBlobUrls;

        private bool _disposed;

        public SelectedFileReader(HttpClient http,
            SelectedFile file, 
            FileBlobUrls fileBlobUrls)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            File = file ?? throw new ArgumentNullException(nameof(file));
            _fileBlobUrls = fileBlobUrls ?? throw new ArgumentNullException(nameof(fileBlobUrls));
        }

        public SelectedFile File { get; private set; }

        public async Task<string> GetFileBlobUrlAsync() => await _fileBlobUrls.GetFileBlobUrl(File.Name);

        public async Task<byte[]> GetFileBytesAsync()
        {
            string customBlobUrl = await _GetCustomBlobFetchAsync();
            return await _http.GetByteArrayAsync(customBlobUrl);
        }

        public async Task<Stream> GetFileStreamAsync()
        {
            string customBlobUrl = await _GetCustomBlobFetchAsync();
            return await _http.GetStreamAsync(customBlobUrl);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _customBlobFetch?.Dispose();
                _fileBlobUrls?.Dispose();
            }
        }

        private async Task<string> _GetCustomBlobFetchAsync()
        {
            if (_customBlobFetch is null)
                _customBlobFetch = new CustomBlobFetch(await GetFileBlobUrlAsync());
            return _customBlobFetch.CustomBlobUrl;
        }
    }
}
