using System.IO.Abstractions;

namespace LehmanLaidun.FileSystem
{
    public class LogicFactory
    {
        private LogicFactory()
        {
        }

        /// <summary>This method creates a <see cref="Logic"/> object for anywhere in a folder structure.
        /// </summary>
        /// <param name="fileSystem">The FileSystem object, typically System.IO.FileSystem for production.</param>
        /// <param name="path">Any path, rooted or not.</param>
        /// <param name="pluginHandler">A handler for plugins.</param>
        /// <returns></returns>
        public static Logic CreateForPath(IFileSystem fileSystem, string path, IPluginHandler? pluginHandler)
        {
            return Logic.Create(fileSystem, path/*, pluginHandler*/);
        }
    }
}
