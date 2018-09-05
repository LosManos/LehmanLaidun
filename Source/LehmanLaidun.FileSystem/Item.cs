namespace LehmanLaidun.FileSystem
{
    public abstract class FileSystemItem
    {

    }

    public class DirectoryItem : FileSystemItem
    {
        public string Name { get; }

        private DirectoryItem(string name)
        {
            Name = name;
        }
        
        public static DirectoryItem Create(string name)
        {
            return new DirectoryItem(name);
        }
    }

    public class FileItem : FileSystemItem
    {
        public string Path { get; }
        public string Name { get; }

        private FileItem(string path, string name)
        {
            Path = path;
            Name = name;
        }

        public static FileItem Create(string path, string name)
        {
            return new FileItem(path, name);
        }
    }
}
