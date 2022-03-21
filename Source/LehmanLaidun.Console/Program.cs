// Note: We should *not* use System.IO but instead System.IO.Abstractions;
//using System.IO;
using CommandLine;
using CompulsoryCow.AssemblyAbstractions;
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
        [System.Obsolete("TODO:OF:Remove")]
        public string PluginFiles { get; set; } = "";

        [Option("processors", HelpText = "List of external console programs that can add information. Separated by space.")] // We don't yet handle spaces in path/filename.
        public string Processors { get; set; } = "";

        // TODO:OF:Can we get rid of explicit "verbose"? and the the built-in. If it exists? https://github.com/commandlineparser/commandline
        [Option("verbose")]
        public bool Verbose { get; set; }
    }

    /// <summary>This class is internal only to make unit tests work.
    /// </summary>
    internal class Program
    {
        /// <summary>This method is internal only to make tests work.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static Options ParseArgs(string[] args)
        {
            Options options = new Options();
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o => options = o);
            return options;
        }

        /// <summary>This class is internal only to make tests work.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static int Main(string[] args)
        {
            if(args == null || args.Length == 0)
            {
                return (int)ReturnValues.NoInput;
            }

            var options = ParseArgs(args);

            var impl = new ProgramImpl(
                //PluginHandler.Create(),
                new System.IO.Abstractions.FileSystem(),
                //new AssemblyFactory(),
                Outputter.Create()); ;

            var res = impl.Execute(options);

            return (int)res;
        }
    }
}
