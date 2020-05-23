using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace W8lessLabs.Blazor.LocalFiles
{
    public class FileBlobContainer : IDisposable
    {
        private bool _disposed;
        private Dictionary<string, (bool revoked, string fileBlobUrl)> _fileUrls;
        private IJSRuntime _jsRuntime;
        private ElementReference _fileSelect;

        public FileBlobContainer(IJSRuntime jsRuntime, ElementReference fileSelect)
        {
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            _fileSelect = fileSelect;
            _fileUrls = new Dictionary<string, (bool revoked, string fileBlobUrl)>(StringComparer.OrdinalIgnoreCase);
        }

        async Task<string> CreateFileBlobUrl(string selectedFileName) =>
            await (_jsRuntime).InvokeAsync<string>("blazorLocalFiles.createFileUrl", selectedFileName, _fileSelect)
                .ConfigureAwait(false);

        async Task RevokeFileBlobUrl(string fileBlobUrl) =>
            await (_jsRuntime).InvokeAsync<object>("blazorLocalFiles.revokeFileUrl", fileBlobUrl)
                .ConfigureAwait(false);

        

        public async Task<string> GetFileBlobUrlAsync(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                if (_fileUrls.TryGetValue(fileName, out (bool revoked, string fileBlobUrl) url))
                {
                    if (url.revoked)
                        throw new InvalidOperationException("File Blob Url " + url.fileBlobUrl + " has already been revoked.");
                    else
                        return url.fileBlobUrl;
                }
                else
                {
                    (bool revoked, string fileBlobUrl) createdUrl = (false, await CreateFileBlobUrl(fileName).ConfigureAwait(false));
                    if(createdUrl.fileBlobUrl is null)
                        throw new NullReferenceException("Unable to create File Blob Url for file: " + fileName);
                    _fileUrls[fileName] = createdUrl;
                    return createdUrl.fileBlobUrl;
                }
            }
            return default;
        }

        public async Task ResetAsync()
        {
            await _RevokeAll().ConfigureAwait(false);
            _fileUrls.Clear();
        }

        private async Task _RevokeAll()
        {
            string[] fileNames = _fileUrls.Keys.ToArray();
            foreach (var fileName in fileNames)
            {
                (bool revoked, string fileBlobUrl) url = _fileUrls[fileName];
                if (!url.revoked)
                {
                    try
                    {
                        await RevokeFileBlobUrl(url.fileBlobUrl).ConfigureAwait(false);
                        _fileUrls[fileName] = (true, url.fileBlobUrl); // mark revoked
                    }
                    catch (Exception ex) { Console.WriteLine("Exception revoking File Blob Url " + url.fileBlobUrl + " Error: " + ex.Message); }
                }
            }
        }


        // TODO remove once async Dispose is usable
        void RevokeFileBlobUrlSynchronous(string fileBlobUrl) =>
            ((IJSInProcessRuntime)_jsRuntime).Invoke<object>("blazorLocalFiles.revokeFileUrl", fileBlobUrl);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;

                // TODO make async once Blazor supports it and use RevokeAll method...
                //await _RevokeAll();
                
                foreach (var file in _fileUrls)
                {
                    string fileName = file.Key;
                    (bool revoked, string fileBlobUrl) url = file.Value;
                    if (!url.revoked)
                    {
                        try
                        {
                            RevokeFileBlobUrlSynchronous(url.fileBlobUrl);
                        }
                        catch (Exception ex) { Console.WriteLine("Exception revoking File Blob Url " + url.fileBlobUrl + " Error: " + ex.Message); }
                    }
                }

                _fileUrls = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
