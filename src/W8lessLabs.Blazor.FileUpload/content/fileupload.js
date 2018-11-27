if (!window.W8lessLabs) {
    window.W8lessLabs = {};
}
if (!window.W8lessLabs.Blazor) {
    window.W8lessLabs.Blazor = {};
}

window.W8lessLabs.Blazor.FileUpload = {
    customBlobUrls: { count: 0 },

    showFileSelector: function (element, component) {

        element.onchange = function () {
            var files = element.files;
            var i, fileList, file;

            if (files.length > 0) {
                fileList = [];
                for (i = 0; i < files.length; i++) {
                    file = files[i];
                    fileList.push({
                        name: file.name,
                        size: file.size,
                        type: file.type
                    });
                }
                component.invokeMethodAsync('FilesSelected', fileList);
            }
        };
        element.click();
    },

    createFileUrl: function (fileName, element) {
        var files = element.files;
        var i, file;

        if (files.length > 0) {
            for (i = 0; i < files.length; i++) {
                file = files[i];
                if (file.name === fileName) {
                    return window.URL.createObjectURL(file);
                }
            }
        }
        return null;
    },

    revokeFileUrl: function (fileUrl) {
        window.URL.revokeObjectURL(fileUrl);
    },

    configureBlobFetch: function (customBlobUrl) {
        var f = window.fetch;
        var customBlobUrls = window.W8lessLabs.Blazor.FileUpload.customBlobUrls;

        if (!customBlobUrls.hasOwnProperty(customBlobUrl)) {
            customBlobUrls[customBlobUrl] = customBlobUrl;
            customBlobUrls.count += 1;
        }

        if (!f.isCustomBlobFetch) {
            var customFetch = function (url, headers) {
                if (customBlobUrls.hasOwnProperty(url)) {
                    if (url && url.indexOf('?wasm_blob') !== -1) {
                        url = 'blob:' + url.replace('?wasm_blob', ''); // change http://url?wasm_blob -> blob:http://url
                    }
                }
                return f(url, headers);
            };
            customFetch.isCustomBlobFetch = true;
            customFetch.originalFetch = f;
            window.fetch = customFetch;
        }
        
    },

    revertBlobFetch: function (customBlobUrl) {
        var customBlobUrls = window.W8lessLabs.Blazor.FileUpload.customBlobUrls;
        if (customBlobUrls.hasOwnProperty(customBlobUrl)) {
            delete customBlobUrls[customBlobUrl];
            customBlobUrls.count = Math.max(0, customBlobUrls.count - 1);
        }
        if (customBlobUrls.count === 0 && window.fetch.isCustomBlobFetch) {
            window.fetch = window.fetch.originalFetch;
        }
    }
};
