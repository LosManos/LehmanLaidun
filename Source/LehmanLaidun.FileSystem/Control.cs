using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

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
            var ret = new List<FileItem>();

            var directories = FileSystem.Directory.EnumerateDirectories(Path, "*", System.IO.SearchOption.AllDirectories);
            foreach( var directory in directories)
            {
                var files = FileSystem.Directory.EnumerateFiles(directory, "*", System.IO.SearchOption.TopDirectoryOnly);
                foreach( var file in files)
                {
                    ret.Add(FileItem.Create(directory, file));
                }
            }
            return ret;
		}
	}
}
