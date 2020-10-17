using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace W8lessLabs.Blazor.LocalFiles
{
    public partial class FileSelect : IAsyncDisposable
    {
        public const string DefaultAccept = "*/*";

        [Inject]
        IServiceProvider Services { get; set; }

        [Inject]
        IJSRuntime JsRuntime { get; set; }

        [Inject]
        HttpClient Http { get; set; }

        [Parameter]
        public string Accept { get; set; } = DefaultAccept;

        [Parameter]
        public bool IsMultiple { get; set; }

#pragma warning disable CS0649
        ElementReference fileSelect;
#pragma warning restore CS0649
        SelectedFile[] selectedFiles;
        FileBlobContainer blobContainer;

        public bool HasSelectedFiles => selectedFiles?.Length > 0;

        public IEnumerable<SelectedFile> SelectedFiles => selectedFiles ?? Array.Empty<SelectedFile>();

        [Parameter]
        public EventCallback<SelectedFile[]> FilesSelected { get; set; }

        /// <summary>
        /// Fires when the user finishes selecting one or more files. The FileSelectChangeArgs contains a reference to the FileSelect as well 
        /// as a list of all of the files currently referenced by the FileSelectList.
        /// </summary>
        [Parameter]
        public EventCallback<FileSelectChangeArgs> FilesChanged { get; set; }

        public void SelectFiles()
        {
            ((IJSInProcessRuntime)JsRuntime).InvokeAsync<object>("blazorLocalFiles.showFileSelector", fileSelect, DotNetObjectReference.Create(this));
        }

        [JSInvokable]
        public async Task FilesSelectedAsync(SelectedFile[] files)
        {
            if (files?.Length > 0)
            {
                if (blobContainer is null)
                    blobContainer = new FileBlobContainer(JsRuntime, fileSelect);
                else
                    await blobContainer.ResetAsync().ConfigureAwait(false);

                selectedFiles = files;

                // clone
                var eventArgFiles = new SelectedFile[files.Length];
                Array.Copy(files, eventArgFiles, files.Length);

                await FilesSelected.InvokeAsync(eventArgFiles).ConfigureAwait(false);

                await FilesChanged.InvokeAsync(new FileSelectChangeArgs(this, eventArgFiles));
            }
        }

        public async Task<string> GetFileBlobUrlAsync(string fileName)
        {
            if (blobContainer is null)
            {
                throw new InvalidOperationException("No files have been selected.");
            }
            else
            {
                return await blobContainer.GetFileBlobUrlAsync(fileName).ConfigureAwait(false);
            }
        }

        public async Task<byte[]> GetFileBytesAsync(string fileName)
        {
            if (blobContainer is null)
            {
                throw new InvalidOperationException("No files have been selected.");
            }
            else
            {
                return await Http.GetByteArrayAsync(
                    await blobContainer.GetFileBlobUrlAsync(fileName).ConfigureAwait(false)).ConfigureAwait(false);
            }
        }

        public async Task<Stream> OpenFileStreamAsync(string fileName)
        {
            if (blobContainer is null)
            {
                throw new InvalidOperationException("No files have been selected.");
            }
            else
            {
                return await Http.GetStreamAsync(
                    await blobContainer.GetFileBlobUrlAsync(fileName).ConfigureAwait(false)).ConfigureAwait(false);
            }
        }

        private bool disposed;
        public virtual ValueTask DisposeAsync()
        {
            if(!disposed)
            {
                disposed = true;

                if (blobContainer != null)
                    return blobContainer.DisposeAsync();
            }
            return ValueTask.CompletedTask;
        }
    }
}
