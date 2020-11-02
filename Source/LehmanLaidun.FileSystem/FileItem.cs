using LehmanLaidun.Plugin;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace LehmanLaidun.FileSystem
{
    public class FileItem : FileSystemItem
    {
        public string Name { get; }
        public string Path { get; }
        public IEnumerable<ParseResult> Data { get; private set; }

        private FileItem(string path, string name, IEnumerable<ParseResult> parseResults) //, long length, DateTime lastWriteTime)
        {
            Path = path;
            Name = name;
            Data = parseResults;
        }

        internal static FileItem Create(
            IFileSystem fileSystem, 
            string pathFile, 
            IPluginHandler pluginHandler)
        {
            // There is a problem with `GetDirectoryName` as it cuts the drive name if there is no directory or file.
            // That should not be a problem here though as we always get a file.
            // https://docs.microsoft.com/en-us/dotnet/api/system.io.path.getdirectoryname
            // TODO:Can/should we change to Abstraction.
            string path = System.IO.Path.GetDirectoryName(pathFile);

            // TODO:Can/should we change to Abstraction.
            string filename = System.IO.Path.GetFileName(pathFile);

            //// We cannot use new FileInfo(...).Length as it throws an exception.
            //// See here: https://stackoverflow.com/questions/44029830/how-do-i-mock-the-fileinfo-information-for-a-file
            //long length = fileSystem.FileInfo.FromFileName(pathFile).Length;

            //// GetLastWriteTime always returns the value as local kind
            //// so we change it to UTC to alway have... UTC.
            //var lastWriteTime = fileSystem.File.GetLastWriteTime(pathFile).ToUniversalTime();

            var results = pluginHandler.Execute(pathFile);

            return new FileItem(path, filename, results); //, length, lastWriteTime);
        }

        public override bool Equals(object obj)
        {
            var item = obj as FileItem;
            return item != null &&
                   Path == item.Path &&
                   Name == item.Name &&
                   Enumerable.SequenceEqual(Data, item.Data);
        }

        public override int GetHashCode()
        {
            var hashCode = -1534044191;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            if( Data != null)
            {
                foreach( var datum in Data)
                {
                    hashCode = hashCode * -1521134295 + EqualityComparer<ParseResult>.Default.GetHashCode(datum);
                }
            }
            return hashCode;
        }

        public static bool operator ==(FileItem? item1, FileItem? item2)
        {
            return EqualityComparer<FileItem?>.Default.Equals(item1, item2);
        }

        public static bool operator !=(FileItem? item1, FileItem? item2)
        {
            return !(item1 == item2);
        }
    }
}