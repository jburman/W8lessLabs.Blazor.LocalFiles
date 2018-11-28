using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace W8lessLabs.Blazor.LocalFiles
{
    public class FileBlobUrls : IDisposable
    {
        private bool _disposed;
        private Dictionary<string, (bool revoked, string fileBlobUrl)> _fileUrls;
        private Func<string, Task<string>> _fileCreator;
        private Func<string, Task> _fileDestructor;

        public FileBlobUrls(
            Func<string, Task<string>> fileCreator,
            Func<string, Task> fileDestructor)
        {
            _fileCreator = fileCreator ?? throw new ArgumentNullException(nameof(fileCreator));
            _fileDestructor = fileDestructor ?? throw new ArgumentNullException(nameof(fileDestructor));

            _fileUrls = new Dictionary<string, (bool revoked, string fileBlobUrl)>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<string> GetFileBlobUrl(string fileName)
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
                    (bool revoked, string fileBlobUrl) createdUrl = (false, await _fileCreator(fileName));
                    if(createdUrl.fileBlobUrl is null)
                        throw new NullReferenceException("Unable to create File Blob Url for file: " + fileName);
                    _fileUrls[fileName] = createdUrl;
                    return createdUrl.fileBlobUrl;
                }
            }
            return default;
        }

        public async Task RevokeFileBlobUrl(string fileName)
        {
            if(!string.IsNullOrEmpty(fileName))
            {
                if (_fileUrls.TryGetValue(fileName, out (bool revoked, string fileBlobUrl) url))
                {
                    if (!url.revoked)
                    {
                        await _fileDestructor(url.fileBlobUrl);
                        _fileUrls[fileName] = (true, url.fileBlobUrl);
                    }
                }
            }
        }

        public void Dispose()
        {
            if(!_disposed)
            {
                _disposed = true;

                foreach((bool revoked, string fileBlobUrl) url in _fileUrls.Values)
                {
                    if (!url.revoked)
                    {
                        try
                        {
                            _fileDestructor(url.fileBlobUrl);
                        } catch (Exception ex) { Console.WriteLine("Exception revoking File Blob Url " + url.fileBlobUrl + " Error: " + ex.Message); }
                    }
                }
                _fileUrls = null;
            }
        }
    }
}
