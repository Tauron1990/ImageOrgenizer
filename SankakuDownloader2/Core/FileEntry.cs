
namespace SankakuDownloader2.Core
{
    public class FileEntry
    {
        public string Name { get; set; }
        public long   Size { get; set; }
        public string Path { get; set; }

        public override string ToString()
        {
            return $"{Name} -- {Size} Bytes -- {Path}";
        }
    }
}