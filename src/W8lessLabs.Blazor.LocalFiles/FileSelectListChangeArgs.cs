namespace W8lessLabs.Blazor.LocalFiles
{
    public class FileSelectListChangeArgs
    {
        public FileSelectListChangeArgs(FileSelectList fileSelectList, SelectedFile[] files)
        {
            FileSelectList = fileSelectList;
            Files = files;
        }

        public FileSelectList FileSelectList { get; }

        public SelectedFile[] Files { get; }
    }
}
