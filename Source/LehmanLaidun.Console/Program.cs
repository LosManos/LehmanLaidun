using System;
using System.Collections.Generic;
// Note: We should *not* use System.IO but instead System.IO.Abstractions;
//using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CommandLine;
using LehmanLaidun.FileSystem;
using C = System.Console;

namespace LehmanLaidun.Console
{
    enum ReturnValues
    {
        Success = 0,
        NoInput = 1,
        InvalidMyDirectory = 2,
        InvalidTheirsDirectory = 3
    }

    class Options
    {
        [Option("mypath", Required = true, HelpText = "A path to compare with another.")]
        public string MyPath { get; set; } = "";

        [Option("theirpath", HelpText = "Another path to compare.")]
        public string TheirPath { get; set; } = "";

        [Option("ox", HelpText = "Use to Output in Xml format.")]
        public bool OutputXml { get; set; }

        [Option("pluginpath", HelpText = "Path of plugins. It looks for /PluginName/PluginName.dll")]
        public string PluginPath { get; set; } = "";

        [Option("pluginfiles", HelpText = "List of plugin dlls. Separated by space.")]
        public string PluginFiles { get; set; } = "";

        [Option("verbose")]
        public bool Verbose { get; set; }
    }

    class Program
    {
        static System.IO.Abstractions.FileSystem fileSystem = new System.IO.Abstractions.FileSystem();

        static int Main(string[] args)
        {
            Options options = new Options();
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o => options = o);

            Func<string> executingFolder = () => fileSystem.Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase!).LocalPath)!;
            Func<string, string> rootedPath = (string path) => fileSystem.Path.GetFullPath(path)!;
            Func<string, string[]> pluginFolders = (string pluginPath) => fileSystem.Directory.GetDirectories(pluginPath);

            if (options.Verbose)
            {
                OutputOptions(options);
                Output(
                        ("PluginFolders", () => string.Join(',', pluginFolders(options.PluginPath)))
                );
            }

            if (false == TryDirectoryExists(options.MyPath, out var myFilesRoot))
            {
                return (int)ReturnValues.InvalidMyDirectory;
            }

            var pluginHandler = PluginHandler.Create();

            //  Below here we are allowed to touch the file system.

            if(options.Verbose)
            {
                Output("ExecutingFolder", executingFolder);
            }

            if (options.PluginPath != string.Empty)
            {
                var pluginPaths = pluginFolders(options.PluginPath).Select(p => fileSystem.Path.Combine(rootedPath(p), fileSystem.DirectoryInfo.FromDirectoryName(p).Name + ".dll"));
                Output("Plugins paths", () => pluginPaths);

                var assemblies = pluginPaths.Select(p => Assembly.LoadFile(p));

                pluginHandler.Load(assemblies);
            }

            if( options.PluginFiles != string.Empty)
            {
                //  We don't handle paths and files with space in them. Yet.
                var pluginFiles = options.PluginFiles.Split(" ");
                Output("Plugin files", () => pluginFiles);

                var assemblies = pluginFiles.Select(p => Assembly.LoadFile(p));

                pluginHandler.Load(assemblies);
            }

            if (options.TheirPath == "")
            {
                var files = LogicFactory.CreateForPath(
                    fileSystem,
                    myFilesRoot!,
                    pluginHandler).AsXDocument();
                OutputResult(files);
            }
            else
            {
                if (false == TryDirectoryExists(options.TheirPath, out string? theirFilesRoot))
                {
                    return (int)ReturnValues.InvalidTheirsDirectory;
                }

                var myFiles = LogicFactory.CreateForPath(new System.IO.Abstractions.FileSystem(), myFilesRoot!, pluginHandler).AsXDocument();
                var theirFiles = LogicFactory.CreateForPath(new System.IO.Abstractions.FileSystem(), theirFilesRoot!, pluginHandler).AsXDocument();

                var differences = Logic.CompareXml(myFiles, theirFiles, new[] { "name", "length" });

                //TODO:Make other output if differences.Result == true;

                OutputResult(differences);
            }

            return (int)ReturnValues.Success;
        }

        private static void OutputOptions(Options options)
        {
            C.WriteLine("Options:");
            C.WriteLine($"Path1:{options.MyPath}.");
            C.WriteLine($"Path2:{options.TheirPath}.");
            C.WriteLine($"OutputXml:{ options.OutputXml}.");
        }

        private static void Output(params (string key, Func<string> valueFunc)[] kvps)
        {
            foreach (var kvp in kvps)
            {
                Output(kvp.key, kvp.valueFunc);
            }
        }

        private static void Output(string key, Func<IEnumerable<string>> valueFunc)
        {
            Output(key, () => string.Join(',', valueFunc()));
        }

        private static void Output(string key, Func<string> valueFunc)
        {
            C.WriteLine($"{key}:{valueFunc()}");
        }

        private static void OutputResult(XDocument files)
        {
            C.WriteLine(files.ToString());
        }

        private static void OutputResult((bool Result, IEnumerable<Difference> Differences) diff)
        {
            foreach (var d in diff.Differences)
            {
                if (d.FirstXPath != null)
                {
                    C.WriteLine($"Found only in first:{d.FirstXPath}.");
                }
                if (d.SecondXPath != null)
                {
                    C.WriteLine($"Found only in second:{d.SecondXPath}.");
                }
            }
        }

        private static bool TryDirectoryExists(string possibleDirectory, out string? validDirectory)
        {
            if (System.IO.Directory.Exists(possibleDirectory))
            {
                validDirectory = possibleDirectory;
                return true;
            }
            validDirectory = null;
            return false;
        }
    }
}
