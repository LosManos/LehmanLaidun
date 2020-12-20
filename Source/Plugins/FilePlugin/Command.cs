using LehmanLaidun.Plugin;
using System.IO.Abstractions;
using System.Xml.Linq;

namespace FilePlugin
{
    public class Command : ICommand
    {
        public string Name => "File plugin";

        public string Description => "This plugin returns file info from the system.";

        private IFileSystem fileSystem;

        public Command()
        {
            fileSystem = new FileSystem();
        }

        public ParseResult Parse(string pathfile)
        {
            // There is a problem with `GetDirectoryName` as it cuts the drive name if there is no directory or file.
            // That should not be a problem here though, as we always get a file.
            // https://docs.microsoft.com/en-us/dotnet/api/system.io.path.getdirectoryname
            var path = System.IO.Path.GetDirectoryName(pathfile);

            var filename = System.IO.Path.GetFileName(pathfile);

            // We cannot use new FileInfo(...).Length as it throws an exception.
            // See here: https://stackoverflow.com/questions/44029830/how-do-i-mock-the-fileinfo-information-for-a-file
            var length = fileSystem.FileInfo.FromFileName(pathfile).Length;

            // GetLastWriteTime always returns the value as local kind
            // so we change it to UTC to alway have... UTC.
            var lastWriteTime = fileSystem.File.GetLastWriteTime(pathfile).ToUniversalTime();

            var xml = new XDocument(
                new XElement("file", 
                    new XAttribute("name", filename),
                    new XAttribute("path", path ?? string.Empty),
                    new XAttribute("length", length),
                    new XAttribute("lastWriteTime", lastWriteTime)
                )
            );

            return ParseResult.Create(
                Name,
                xml
            );

        }
    }
}
    