using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace W8lessLabs.Blazor.LocalFiles
{
    public partial class FileSelectList : IAsyncDisposable
    {
        [Parameter]
        public string Accept { get; set; } = FileSelect.DefaultAccept;

        [Parameter]
        public bool IsMultiple { get; set; }

        int selectionCount = 0;
#pragma warning disable CS0649
        FileSelect fileSelect;
#pragma warning restore CS0649
        private Dictionary<string, (SelectedFile file, FileSelect selector)> fileSelectors = 
            new Dictionary<string, (SelectedFile file, FileSelect selector)>(StringComparer.OrdinalIgnoreCase);
        private LinkedList<FileSelect> disposeList = new LinkedList<FileSelect>();

        public bool HasSelectedFiles => fileSelectors.Count > 0;

        public IEnumerable<SelectedFile> SelectedFiles => fileSelectors.Select(fs => fs.Value.file).ToArray();

        /// <summary>
        /// Fires when the user finishes selecting one or more files. The SelectedFile[] argument represents the most recent selected files, and 
        /// it does not include previously selected files. (in contrast to the FileListChanged event, which does return the complete set of all selected files.)
        /// </summary>
        [Parameter]
        public EventCallback<SelectedFile[]> FilesSelected { get; set; }

        /// <summary>
        /// Fires when the user finishes selecting one or more files. The FileSelectListChangeArgs contains a reference to the FileSelectList as well 
        /// as a list of all of the files currently referenced by the FileSelectList.
        /// </summary>
        [Parameter]
        public EventCallback<FileSelectListChangeArgs> FileListChanged { get; set; }

        public void SelectFiles()
        {
            fileSelect.SelectFiles();
        }

        async Task FilesSelectedHandlerAsync(SelectedFile[] files)
        {
            if (files?.Length > 0)
            {
                foreach (var file in files)
                    fileSelectors[file.Name] = (file, fileSelect);
                disposeList.AddLast(fileSelect);

                await FilesSelected.InvokeAsync(files).ConfigureAwait(false);

                await FileListChanged.InvokeAsync(new FileSelectListChangeArgs(this, (SelectedFile[])SelectedFiles));
            }

            selectionCount++;
        }

        public void RemoveFile(string fileName)
        {
            if (fileSelectors.ContainsKey(fileName))
                fileSelectors.Remove(fileName);
        }

        public async Task<string> GetFileBlobUrlAsync(string fileName)
        {
            if (fileSelectors.TryGetValue(fileName, out (SelectedFile file, FileSelect selector) fileSelect))
            {
                return await fileSelect.selector.GetFileBlobUrlAsync(fileName).ConfigureAwait(false);
            }
            throw new ArgumentOutOfRangeException("File not found in selected files list: " + fileName);
        }

        public async Task<byte[]> GetFileBytesAsync(string fileName)
        {
            if (fileSelectors.TryGetValue(fileName, out (SelectedFile file, FileSelect selector) fileSelect))
            {
                return await fileSelect.selector.GetFileBytesAsync(fileName).ConfigureAwait(false);
            }
            throw new ArgumentOutOfRangeException("File not found in selected files list: " + fileName);
        }

        public async Task<Stream> OpenFileStreamAsync(string fileName)
        {
            if(fileSelectors.TryGetValue(fileName, out (SelectedFile file, FileSelect selector) fileSelect))
            {
                return await fileSelect.selector.OpenFileStreamAsync(fileName).ConfigureAwait(false);
            }
            throw new ArgumentOutOfRangeException("File not found in selected files list: " + fileName);
        }

        private bool disposed;

        public virtual async ValueTask DisposeAsync()
        {
            if (!disposed)
            {
                disposed = true;

                foreach (var fileSelect in disposeList)
                    await fileSelect.DisposeAsync().ConfigureAwait(false);

                disposeList.Clear();
                fileSelectors.Clear();
            }
        }
    }
}
