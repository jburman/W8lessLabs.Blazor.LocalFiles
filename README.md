## Local file support for Blazor (Wasm only) [![NuGet](https://img.shields.io/nuget/v/W8lessLabs.Blazor.LocalFiles.svg)](https://www.nuget.org/packages/W8lessLabs.Blazor.LocalFiles/)

The LocalFiles library contains a Blazor component that makes it super simple to load local files into your .NET code running on Wasm.
Unleash your .NET code to do all kinds of wonderful things with files - e.g. parsing, scanning, modifying etc. without ever having to send the data to a server first!

**Note**: This component is currently limited to Wasm use only. For an alternative approach that supports both 
client side and server side file inputs, see Steve Sanderson's [BlazorInputFile component](http://blog.stevensanderson.com/2019/09/13/blazor-inputfile/).

## Getting Started

See the [test project](https://github.com/jburman/W8lessLabs.Blazor.LocalFiles/tree/master/test/W8lessLabs.Blazor.LocalFilesTest) for several working examples with code. 
There is now also a [live demo site](https://jburman.github.io/BlazorLocalFilesExample/) on Github Pages.

First, install the [W8lessLabs.Blazor.LocalFiles nuget package](https://www.nuget.org/packages/W8lessLabs.Blazor.LocalFiles).

**If you are upgrading from a Preview release,** please see the Change Log below.

```
dotnet add package W8lessLabs.Blazor.LocalFiles --version 3.0.0
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
<FileSelect @ref="fileSelect" FilesSelected="FilesSelectedHandler"></FileSelect>
```

The FileSelect component is a non-visual component that will wire up the necessary plumbing to select and open files. Next, bind an event handler to respond to the file selections.


```
@* onclick triggers the file selector's file picker dialog *@
<button @onclick="SelectFiles">Select Files</button>

@code 
{
    // Component reference
    FileSelect fileSelect;
    
    void SelectFiles() =>
        fileSelect.SelectFiles();

    // Handle the file selection event
    async Task FilesSelectedHandler(SelectedFile[] selectedFiles)
    {
        // example of opening a selected file...
        var selectedFile = selectedFiles[0];
        using (var fileStream = await fileSelect.OpenFileStreamAsync(selectedFile.Name))
        {
            // read from file stream here...
        }

        // alternatively, load all the bytes at once
        var fileBytes = await fileSelect.GetFileBytesAsync(selectedFile.Name);
        
        // or, get a retrieve the underlying blob url
        string fileBlobUrl = await fileSelect.GetFileBlobUrlAsync(selectedFile.Name);
    }
}
```
## Configuration Options
Without any additional configuration (as in the example above), you'll get a file picker that allows a single file to be selected with any extension. This behavior can be controlled via the **IsMultiple** and **Accept** properties, respectively.

```
<FileSelect @ref="imageFileSelect" IsMultiple="true" Accept=".jpg,.png" FilesSelected="ImagesSelected"></FileSelect>
```
The file selector above allows multiple files to be selected at once, and filters to .jpg and .png file extensions.

- Reference for Accept values: https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/file#attr-accept

For a more detailed example [see the Test project](https://github.com/jburman/W8lessLabs.Blazor.LocalFiles/tree/master/test/W8lessLabs.Blazor.LocalFilesTest) in GitHub.

## FileSelectList
New with 1.0.0 release, is a **FileSelectList** component that acts as a re-usable file selector. The regular FileSelect is designed to 
only remember the last set of files that the user selected. The FileSelectList component provides a similar API surface but acts as 
container of selected files that is appended to each time SelectFiles is called.

**Note** if the user selects files with the same name, then previous entries are overwritten.

```
<FileSelectList @ref="fileSelectList" FilesSelected="FilesSelectedHandler"></FileSelectList>
```

See the [Test Page](https://raw.githubusercontent.com/jburman/W8lessLabs.Blazor.LocalFiles/master/test/W8lessLabs.Blazor.LocalFilesTest/Pages/FileList.razor) for example code for using the FileSelectList.


## Technical Details
Under the covers, the LocalFiles component is using a vanilla file input element and 
creates blob: file URLs ([https://www.w3.org/TR/FileAPI/#url](https://www.w3.org/TR/FileAPI/#url).) 
The contents of the files are then retrieved using the browser's Fetch API and passing in the blob: URLs.


## Change Log

### From v2.0.0 to v3.0.0
- Update to .NET 6.0. 
- No functionality changes

### From v1.0.1 to v2.0.0
- Update to .NET 5.0. 
- No functionality changes but a few internal improvements. Takes advantage of Blazor's new async dispose for example.

### From v1.0.0 to v1.0.1
- Added new FileSelect.FilesChanged and FileSelectList.FileListChanged event callbacks
- The new events provide an args object with a reference to the FileSelect (or FileSelectList) as well as to
the list of selected files.

**Example of new Event Callback**
```
<FileSelect @ref="fileSelect" FilesChanged="FilesChangedHandler" />

<button @onclick="@(() => fileSelect.SelectFiles())">Select Files</button>

@code
{
    FileSelect fileSelect;

    async Task FilesChangedHandler(FileSelectChangeArgs args)
    {
        var file = args.Files.First();
        using (var fileStream = await args.FileSelect.OpenFileStreamAsync(file.Name))
        {
            // do something with file contents ...
        }
    }
}
```

### From Preview to v1.0
- Replaced Event and callback options with more standard EventCallback.
- Removed SelectedFileReader class as the FileSelect component now automatically handles file element disposal.
- Moved methods previously available on the SelectedFileReader directly onto the FileSelect component.
- Added new FileSelectList component that is designed as a re-usable file selector that builds up a list of selected files (and allows removal).
