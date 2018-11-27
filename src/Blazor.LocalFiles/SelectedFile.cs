using System;

namespace Blazor.LocalFiles
{
    public class SelectedFile
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public DateTimeOffset LastModified { get; set; }
    }
}
