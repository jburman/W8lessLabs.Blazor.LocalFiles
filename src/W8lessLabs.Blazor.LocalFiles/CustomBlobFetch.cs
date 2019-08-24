using Microsoft.JSInterop;
using System;

namespace W8lessLabs.Blazor.LocalFiles
{
    internal class CustomBlobFetch : IDisposable
    {
        private bool _disposed;
        private IJSInProcessRuntime _jsRuntime;

        public CustomBlobFetch(IJSInProcessRuntime jsRuntime, string blobUrl)
        {
            _jsRuntime = jsRuntime;

            // Remove blob: scheme from URL as HttpRequestMessage does not allow it.
            // Append ?wasm_blob target to end of the url so that customized fetch recognizes at as a blob request.
            if (blobUrl.StartsWith("blob:"))
                blobUrl = blobUrl.Substring("blob:".Length) + "?wasm_blob";

            CustomBlobUrl = blobUrl;

            _jsRuntime.InvokeAsync<object>("blazorLocalFiles.configureBlobFetch", CustomBlobUrl);
        }

        public string CustomBlobUrl { get; private set; }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _jsRuntime.InvokeAsync<object>("blazorLocalFiles.revertBlobFetch", CustomBlobUrl);
            }
        }
    }
}
