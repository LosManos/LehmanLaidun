// Note: We should *not* use System.IO but instead System.IO.Abstractions;
//using System.IO;
using CommandLine;
using LehmanLaidun.FileSystem;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LehmanLaidun.Console.Unit.Test")]
namespace LehmanLaidun.Console
{
    /// <summary>These are the return values.
    /// The enum is internal only to make tests work.
    /// </summary>
    internal enum ReturnValues
    {
        Success = 0,
        NoInput = 1,
        InvalidMyDirectory = 2,
        InvalidTheirsDirectory = 3
    }

    internal class Options
    {
        [Option("mypath", Required = true, HelpText = "A path to compare with another.")]
        public string MyPath { get; set; } = "";

        [Option("theirpath", HelpText = "Another path to compare.")]
        public string TheirPath { get; set; } = "";

        [Option("ox", HelpText = "Use to Output in Xml format.")]
        public bool OutputXml { get; set; }

        [Option("pluginfiles", HelpText = "List of plugin dlls. Separated by space. Each plugin dll must be accompanied by a manifest file.")]
        public string PluginFiles { get; set; } = "";

        [Option("verbose")]
        public bool Verbose { get; set; }
    }

    /// <summary>This class is internal only to make unit tests work.
    /// </summary>
    internal class Program
    {
        private static Options options = new Options();

        /// <summary>This class is internal only to make tests work.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static int Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o => options = o);

            var impl = new ProgramImpl(
                options,
                PluginHandler.Create(),
                new System.IO.Abstractions.FileSystem());

            var res = impl.Execute();

            return (int)res;
        }
    }
}
