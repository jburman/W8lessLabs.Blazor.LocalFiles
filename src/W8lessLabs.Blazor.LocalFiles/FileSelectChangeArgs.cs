using System;

namespace W8lessLabs.Blazor.LocalFiles
{
    public class FileSelectChangeArgs : EventArgs
    {
        public FileSelectChangeArgs(FileSelect fileSelect, SelectedFile[] files)
        {
            FileSelect = fileSelect;
            Files = files;
        }

        public FileSelect FileSelect { get; }

        public SelectedFile[] Files { get; }
    }
}
