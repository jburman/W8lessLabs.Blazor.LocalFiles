## Blazor LocalFiles Component

LocalFiles is a Blazor component that makes it super simple to load local files into your .NET code running on WASM.
Now there are all kind of wonderful things you can do with files, like parsing, scanning, modifying etc. without ever having to send the data to a server first!

## Getting Started

First, install the [W8lessLabs.Blazor.LocalFiles nuget package](https://www.nuget.org/packages/W8lessLabs.Blazor.LocalFiles).
Then, add the following references in your _ViewImports.cshtml

```
@using W8lessLabs.Blazor.LocalFiles
@addTagHelper *,W8lessLabs.Blazor.LocalFiles
```
Next, in your Blazor .cshtml page or component add the FileSelect component tag.


```
<FileSelect ref="fileSelect"></FileSelect>
```

The FileSelect component is a non-visual component that will wire up the necessary plumbing to select and open files. Next, wire up some code to trigger and handle the file selections.


```
<button onclick="@SelectFiles">Select Files</button>

@functions {
    // Component reference
    FileSelect fileSelect;

    // Trigger the browser's file picker and then handle the callback
    void SelectFiles()
    {
        // Call SelectFiles with a (optional) callback. You can also wire up to a fileSelect.OnFilesSelected event
        fileSelect.SelectFiles(async (selectedFiles) =>
        {
            SelectedFile file = selectedFiles.First();
            // file has Name, Size, LastModified

            // Read the file's contents
            using (var fileReader = fileSelect.GetFileReader(file))
            {
                byte[] fileContent = await fileReader.GetFileBytesAsync();
                // Alternatively - get a stream
                //Stream fileStream = await fileReader.GetFileStreamAsync();

                // You can also get the blob URL created in the Browser
                string fileBlobUrl = await fileReader.GetFileBlobUrlAsync();
            } // When fileReader is Disposed, all of the file blob Urls are also revoked (so use them first!)
        });
    }
}
```
## Configuration Options
Without any additional configuration (as in the example above), you'll get a file picker that allows a single file to be selected with any extension. This behavior can be controlled via the **IsMultiple** and **Accept** properties, respectively.

```
<FileSelect ref="imageFileSelect" IsMultiple="true" Accept=".jpg,.png"></FileSelect>
```
The file selector above allows multiple files to be selected at once, and filters to .jpg and .png file extensions.

- Reference for Accept values: https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/file#attr-accept

For a more detailed example [see the Test project](https://github.com/jburman/W8lessLabs.Blazor.LocalFiles/tree/master/test/Blazor.LocalFilesTest) in GitHub.


## Technical Details
Under the covers, the Blazor.LocalFiles component is using a vanilla file input element, and it is creating blob: file URLs ([https://www.w3.org/TR/FileAPI/#url](https://www.w3.org/TR/FileAPI/#url).) The contents of the files are then retrieved using the browser's Fetch API and passing in the blob: URLs.

However, out of the box, the Mono runtime that Blazor WASM runs on does not support blob: URLs ([issue filed](https://github.com/mono/mono/issues/11681).) So, to work around this limitation, a customized blob URL is created that removes the blob: scheme from the beginning of the URL. This allows the URL to pass through Mono's HttpClient implementation. Then a customized Fetch implementation is used that intercept those requests at the browser level and convert them back into standard blob: URLs that are passed into the out of the box Fetch API. 

To reduce any unexpected side effects, the customized Fetch implementation is only used while the Blazor.LocalFile's SelectedFileReader is being utilized. It is reverted to the browser's out of the box implementation as soon as the SelectedFileReader is disposed. Furthermore, any URLs without the customized blob URL format are passed through as normal. The goal is to be as unobtrusive as possible.