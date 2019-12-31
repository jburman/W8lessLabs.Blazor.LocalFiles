## LocalFiles Component for Blazor (Wasm only) [![NuGet](https://img.shields.io/nuget/v/W8lessLabs.Blazor.LocalFiles.svg)](https://www.nuget.org/packages/W8lessLabs.Blazor.LocalFiles/)

LocalFiles is a Blazor component that makes it super simple to load local files into your .NET code running on Wasm.
Unleash your .NET code to do all kinds of wonderful things with files - e.g. parsing, scanning, modifying etc. without ever having to send the data to a server first!

**Note**: This component is currently limited to Wasm use only. For an alternative approach that supports both 
client side and server side file inputs, see Steve Sanderson's [BlazorInputFile component](http://blog.stevensanderson.com/2019/09/13/blazor-inputfile/).

## Getting Started

(Note: [See the Test project](https://github.com/jburman/W8lessLabs.Blazor.LocalFiles/tree/master/test/W8lessLabs.Blazor.LocalFilesTest) for a working sample.)

First, install the [W8lessLabs.Blazor.LocalFiles nuget package](https://www.nuget.org/packages/W8lessLabs.Blazor.LocalFiles).
```
dotnet add package W8lessLabs.Blazor.LocalFiles --version 1.0.0-preview2
```

Second, add a using reference in your **_Imports.razor**.

```
@using W8lessLabs.Blazor.LocalFiles
```

Update the **index.html** and add a reference to a .js file included in the W8lessLabs.Blazor.LocalFiles package.
```
<script src="_content/W8lessLabs.Blazor.LocalFiles/js/fileupload.js"></script>
```

Next, in your Blazor .cshtml page or component add the **FileSelect** component tag.


```
<FileSelect @ref="fileSelect"></FileSelect>
```

The FileSelect component is a non-visual component that will wire up the necessary plumbing to select and open files. Next, wire up some code to trigger and handle the file selections.


```
<button @onclick="@SelectFiles">Select Files</button>

@code 
{
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
<FileSelect @ref="imageFileSelect" IsMultiple="true" Accept=".jpg,.png"></FileSelect>
```
The file selector above allows multiple files to be selected at once, and filters to .jpg and .png file extensions.

- Reference for Accept values: https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/file#attr-accept

For a more detailed example [see the Test project](https://github.com/jburman/W8lessLabs.Blazor.LocalFiles/tree/master/test/W8lessLabs.Blazor.LocalFilesTest) in GitHub.


## Technical Details
Under the covers, the LocalFiles component is using a vanilla file input element and 
creates blob: file URLs ([https://www.w3.org/TR/FileAPI/#url](https://www.w3.org/TR/FileAPI/#url).) 
The contents of the files are then retrieved using the browser's Fetch API and passing in the blob: URLs.
