using System.IO.Abstractions;

namespace LehmanLaidun.FileSystem
{
    public class LogicFactory
    {
        private LogicFactory()
        {
        }

        public static Logic CreateForPath(IFileSystem fileSystem, string path)
        {
            return Logic.Create(fileSystem, path);
        }
    }
}
