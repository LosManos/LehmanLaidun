using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Linq;

namespace LehmanLaidun.FileSystem
{
    public class Logic
    {
        private IFileSystem _fileSystem;

        public string Path { get; }

        internal static Logic Create(IFileSystem fileSystem, string path)
        {
            return new Logic(fileSystem, path);
        }

        private Logic(IFileSystem fileSystem, string path)
        {
            _fileSystem = fileSystem;
            Path = path;
        }

        public IEnumerable<FileItem> AsEnumerableFiles()
        {
            //  First take care of the files in teh folder asked for.
            foreach (var file
                in _fileSystem.Directory.EnumerateFiles(Path, "*", System.IO.SearchOption.TopDirectoryOnly))
            {
                yield return FileItem.Create(Path, _fileSystem.Path.GetFileName(file));
            }
            //  Then recurse the directories.
            foreach (var directory
                in _fileSystem.Directory.EnumerateDirectories(Path, "*", System.IO.SearchOption.AllDirectories))
            {
                foreach (var file
                    in _fileSystem.Directory.EnumerateFiles(directory, "*", System.IO.SearchOption.TopDirectoryOnly))
                {
                    yield return FileItem.Create(directory, _fileSystem.Path.GetFileName(file));
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
                var directoryNames = relPath.Trim(new[] { _fileSystem.Path.DirectorySeparatorChar }).Split(_fileSystem.Path.DirectorySeparatorChar);
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
