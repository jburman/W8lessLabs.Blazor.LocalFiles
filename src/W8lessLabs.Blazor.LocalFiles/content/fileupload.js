if (!window.blazorLocalFiles) {
    window.blazorLocalFiles = {};
}
window.blazorLocalFiles = {
    customBlobUrls: { count: 0 },

    showFileSelector: function (element, component) {

        element.onchange = function () {
            var files = element.files;
            var i, fileList, file, lastModified;

            if (files.length > 0) {
                fileList = [];
                for (i = 0; i < files.length; i++) {
                    file = files[i];
                    lastModified = new Date(file.lastModified);
                    fileList.push({
                        name: file.name,
                        size: file.size,
                        lastModified: lastModified.toISOString()
                    });
                }
                component.invokeMethodAsync('FilesSelectedAsync', fileList);
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
    }
};
