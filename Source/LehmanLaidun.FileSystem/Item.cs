using System;
using System.IO.Abstractions;

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
        public long Length { get; }

        private FileItem(string path, string name, long length)
        {
            Path = path;
            Name = name;
            Length = length;
        }

        [Obsolete("Use the constructor with IFileSystem instead as this creates an erroneous result due to 'length'.", false)]
        public static FileItem Create(string path, string filename)
        {
            return new FileItem(path, filename, 0); // TODO:Remove or correct this method.
        }

        internal static FileItem Create(
            IFileSystem fileSystem, 
            string pathFile)
        {
            // There is a problem with `GetDirectoryName` as it cuts the drive name if there is no directory or file.
            // That should not be a problem here though as we always get a file.
            // https://docs.microsoft.com/en-us/dotnet/api/system.io.path.getdirectoryname
            string path = System.IO.Path.GetDirectoryName(pathFile);

            string filename = System.IO.Path.GetFileName(pathFile);

            // We cannot use new FileInfo(...).Length as it throws an exception.
            // See here: https://stackoverflow.com/questions/44029830/how-do-i-mock-the-fileinfo-information-for-a-file
            long length = fileSystem.FileInfo.FromFileName(pathFile).Length;

            return new FileItem(path, filename, length);
        }
    }
}