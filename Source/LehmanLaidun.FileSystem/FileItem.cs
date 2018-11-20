using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace LehmanLaidun.FileSystem
{
    public class FileItem : FileSystemItem
    {
        public DateTime LastWriteTime { get; }
        public long Length { get; }
        public string Name { get; }
        public string Path { get; }

        private FileItem(string path, string name, long length, DateTime lastWriteTime)
        {
            Path = path;
            Name = name;
            Length = length;
            LastWriteTime = lastWriteTime;
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

            var lastWriteTime = fileSystem.File.GetLastWriteTime(pathFile);

            return new FileItem(path, filename, length, lastWriteTime);
        }

        public override bool Equals(object obj)
        {
            var item = obj as FileItem;
            return item != null &&
                   Path == item.Path &&
                   Name == item.Name &&
                   Length == item.Length &&
                   LastWriteTime == item.LastWriteTime;
        }

        public override int GetHashCode()
        {
            var hashCode = -1534044191;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Length.GetHashCode();
            hashCode = hashCode * -1521134295 + LastWriteTime.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(FileItem item1, FileItem item2)
        {
            return EqualityComparer<FileItem>.Default.Equals(item1, item2);
        }

        public static bool operator !=(FileItem item1, FileItem item2)
        {
            return !(item1 == item2);
        }
    }
}