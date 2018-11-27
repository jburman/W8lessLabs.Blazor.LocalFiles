using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace W8lessLabs.Blazor.FileUpload
{
    public class FileUploader : IDisposable
    {
        public static Task<string> SelectFiles()
        {
            // Implemented in exampleJsInterop.js
            return JSRuntime.Current.InvokeAsync<string>(
                "W8lessLabs.Blazor.FileUpload.showFileSelector",
                null);
        }

        //public async Task ShowFileSelector() =>
        //await JSRuntime.Current.InvokeAsync<object>("app.showFileSelector", "fileSelect", new DotNetObjectRef(this));


        public void Dispose()
        {
            
        }
    }
}
