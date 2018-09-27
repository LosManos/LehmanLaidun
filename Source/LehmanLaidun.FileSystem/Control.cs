using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Linq;

namespace LehmanLaidun.FileSystem
{
    public class Control
    {
        public Func<string, (IEnumerable<string>, string)> _splitPathFile = (pathFile) =>
        {
            var items = pathFile.Split(System.IO.Path.PathSeparator);
            return (items.Reverse().Skip(1).Reverse(), items.Last());
        };

        private IFileSystem _fileSystem;
        private IFileSystem FileSystem
        {
            get => _fileSystem = _fileSystem ?? new System.IO.Abstractions.FileSystem();
            set => _fileSystem = value;
        }

        public string Path { get; }

        public static Control CreateForPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { throw new ArgumentException("A Path is needed.", nameof(path)); }

            return new Control(path);
        }

        private Control(string path)
        {
            Path = path;
        }

        public Control Inject(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
            return this;
        }

        public IEnumerable<FileItem> AsEnumerableFiles()
        {
            //  First take care of the files in teh folder asked for.
            foreach (var file
                in FileSystem.Directory.EnumerateFiles(Path, "*", System.IO.SearchOption.TopDirectoryOnly))
            {
                yield return FileItem.Create(Path, FileSystem.Path.GetFileName(file));
            }
            //  Then recurse the directories.
            foreach (var directory
                in FileSystem.Directory.EnumerateDirectories(Path, "*", System.IO.SearchOption.AllDirectories))
            {
                foreach (var file
                    in FileSystem.Directory.EnumerateFiles(directory, "*", System.IO.SearchOption.TopDirectoryOnly))
                {
                    yield return FileItem.Create(directory, FileSystem.Path.GetFileName(file));
                }
            }
        }

        public XDocument AsXDocument()
        {
            var doc = new XDocument(new XElement("root", new XAttribute("path", Path)));
            foreach (var file in AsEnumerableFiles())
            {
                //  Remove the first part, the Path.
                var relPath = file.Path.Remove(0, Path.Length);
                var directoryNames = relPath.Trim(new[] { FileSystem.Path.DirectorySeparatorChar }).Split(FileSystem.Path.DirectorySeparatorChar);
                var directoryElement = doc.Root;
                for (var i = 0; i < directoryNames.Length; ++i)
                {
                    var directoryNameList = directoryNames.Take(i + 1); // The first n items.

                    directoryElement = doc.SelectDirectoryElement(directoryNameList) ?? directoryElement.AddDirectoryElement(directoryNameList.Last());

                }
                directoryElement.AddFileElement(file);
            }
            return doc;
        }
    }
}
