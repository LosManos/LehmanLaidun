// Note: We should *not* use System.IO but instead System.IO.Abstractions;
//using System.IO;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
//using System.Reflection;
using System.Xml.Linq;
using LehmanLaidun.FileSystem;
using CompulsoryCow.AssemblyAbstractions;

namespace LehmanLaidun.Console
{
    internal class ProgramImpl
    {
        private readonly IPluginHandler pluginHandler;
        private readonly IFileSystem fileSystem;
        private readonly IAssemblyFactory assemblyFactory;
        private readonly IOutputter outputter;

        internal ProgramImpl(
            IPluginHandler pluginHandler,
            IFileSystem fileSystem,
            IAssemblyFactory assemblyFactory,
            IOutputter outputter)
        {
            this.pluginHandler = pluginHandler;
            this.fileSystem = fileSystem;
            this.assemblyFactory = assemblyFactory;
            this.outputter = outputter;
        }

        internal ReturnValues Execute(Options options)
        {
            Func<string> executingFolder = () => fileSystem.Path.GetDirectoryName(new Uri(assemblyFactory.GetExecutingAssembly().Location!).LocalPath)!;
            Func<string, string> rootedPath = (string path) => fileSystem.Path.GetFullPath(path)!;
            Func<string, string[]> pluginFolders = (string pluginPath) => fileSystem.Directory.GetDirectories(pluginPath);

            if (options.Verbose)
            {
                OutputOptions(options);
            }

            if (false == TryDirectoryExists(options.MyPath, out var myFilesRoot))
            {
                return ReturnValues.InvalidMyDirectory;
            }

            Output("ExecutingFolder", executingFolder, options.Verbose);

            //  Plugin files need a manifest.
            if (options.PluginFiles != string.Empty)
            {
                //  We don't handle paths and files with space in them. Yet.
                var pluginFiles = options.PluginFiles.Split(" ");
                Output("Plugin files", () => pluginFiles, options.Verbose);
                var rootedPluginFiles = pluginFiles.Select(pf => fileSystem.Path.GetFullPath(pf));
                Output("Rooted plugin files", () => rootedPluginFiles, options.Verbose);

                var plugins = rootedPluginFiles.Select(rpf => (
                    RootedPath: fileSystem.Path.GetDirectoryName(rpf),
                    PluginFileName: fileSystem.Path.GetFileName(rpf),
                    ManifestFileName: fileSystem.Path.GetFileNameWithoutExtension(rpf) + ".plugin-manifest.xml"));
                Output("Plugin info", () => plugins.Select(x => $"[{x.RootedPath},{x.PluginFileName},{x.ManifestFileName}]"), options.Verbose);

                var manifestHandler = new ManifestHandler(fileSystem);
                var assemblies = new List<IAssembly>();
                foreach (var plugin in plugins)
                {
                    var manifestPathfile = fileSystem.Path.Combine(plugin.RootedPath, plugin.ManifestFileName);
                    if (fileSystem.File.Exists(manifestPathfile))
                    {
                        var manifest = manifestHandler.Read(manifestPathfile);
                        LoadManifestFiles(manifest, plugin.RootedPath, options.Verbose);
                    }

                    var pluginPathfile = fileSystem.Path.Combine(plugin.RootedPath, plugin.PluginFileName);
                    var loadedPlugin = assemblyFactory.LoadFile(pluginPathfile);
                    assemblies.Add(loadedPlugin);
                }

                pluginHandler.Load(assemblies);
            }

            if (options.TheirPath == "")
            {
                var files = LogicFactory.CreateForPath(
                    fileSystem,
                    myFilesRoot!,
                    pluginHandler).AsXDocument();
                OutputResult(files, outputter);
            }
            else
            {
                if (false == TryDirectoryExists(options.TheirPath, out string? theirFilesRoot))
                {
                    return ReturnValues.InvalidTheirsDirectory;
                }

                var myFiles = LogicFactory.CreateForPath(fileSystem, myFilesRoot!, pluginHandler).AsXDocument();
                var theirFiles = LogicFactory.CreateForPath(fileSystem, theirFilesRoot!, pluginHandler).AsXDocument();

                var differences = Logic.CompareXml(myFiles, theirFiles, new[] { "name", "length" });

                //TODO:Make other output if differences.Result == true;

                OutputResult(differences, outputter);
            }

            return ReturnValues.Success;
        }

        private void LoadManifestFiles(Manifest manifest, string path, bool optionsVerbose)
        {
            foreach (var dependency in manifest.Dependencies)
            {
                var pathFile = fileSystem.Path.Combine(path, dependency.PathFile);
                Output("Dependency pathFIle", () => pathFile, optionsVerbose);
                assemblyFactory.LoadFrom(pathFile);
            }
        }

        private bool TryDirectoryExists(string possibleDirectory, out string? validDirectory)
        {
            if (fileSystem.Directory.Exists(possibleDirectory))
            {
                validDirectory = possibleDirectory;
                return true;
            }
            validDirectory = null;
            return false;
        }

        private void Output(string key, Func<IEnumerable<string>> valueFunc, bool @if = true)
        {
            Output(key, () => string.Join(',', valueFunc()), @if);
        }

        private void Output(string key, Func<string> valueFunc, bool @if = true)
        {
            if (@if)
            {
                outputter.WriteLine($"{key}:{valueFunc()}");
            }
        }

        private void OutputOptions(Options options)
        {
            outputter.WriteLine("Options:");
            outputter.WriteLine($"Path1:{options.MyPath}.");
            outputter.WriteLine($"Path2:{options.TheirPath}.");
            outputter.WriteLine($"OutputXml:{ options.OutputXml}.");
            outputter.WriteLine($"PluginFles:{ options.PluginFiles}.");
            outputter.WriteLine($"Verbose:{ options.Verbose}.");
        }

        private void OutputResult(XDocument files, IOutputter outputter)
        {
            outputter.WriteLine(files.ToString());
        }

        private static void OutputResult((bool Result, IEnumerable<Difference> Differences) diff, IOutputter outputter)
        {
            foreach (var d in diff.Differences)
            {
                if (d.FirstXPath != null)
                {
                    outputter.WriteLine($"Found only in first:{d.FirstXPath}.");
                }
                if (d.SecondXPath != null)
                {
                    outputter.WriteLine($"Found only in second:{d.SecondXPath}.");
                }
            }
        }
    }
}
