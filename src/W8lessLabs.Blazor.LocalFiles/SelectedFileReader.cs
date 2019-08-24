using Microsoft.JSInterop;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace W8lessLabs.Blazor.LocalFiles
{
    public class SelectedFileReader : IDisposable
    {
        private HttpClient _http;
        private FileBlobUrls _fileBlobUrls;
        private IJSInProcessRuntime _jsRuntime;

        private bool _disposed;

        public SelectedFileReader(HttpClient http,
            IJSRuntime jsRuntime,
            SelectedFile file, 
            FileBlobUrls fileBlobUrls)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _jsRuntime = (IJSInProcessRuntime)jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            File = file ?? throw new ArgumentNullException(nameof(file));
            _fileBlobUrls = fileBlobUrls ?? throw new ArgumentNullException(nameof(fileBlobUrls));
        }

        public SelectedFile File { get; private set; }

        public async Task<string> GetFileBlobUrlAsync() => await _fileBlobUrls.GetFileBlobUrl(File.Name);

        public async Task<byte[]> GetFileBytesAsync()
        {
            return await _http.GetByteArrayAsync(
                await GetFileBlobUrlAsync());
        }

        public async Task<Stream> GetFileStreamAsync()
        {
            return await _http.GetStreamAsync(
                await GetFileBlobUrlAsync());
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                //_customBlobFetch?.Dispose();
                _fileBlobUrls?.Dispose();
            }
        }
    }
}
