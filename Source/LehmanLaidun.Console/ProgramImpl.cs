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
using System.Diagnostics;

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

            ////  Plugin files need a manifest.
            //if (options.PluginFiles != string.Empty)
            //{
            //    //  We don't handle paths and files with space in them. Yet.
            //    var pluginFiles = options.PluginFiles.Split(" ");
            //    Output("Plugin files", () => pluginFiles, options.Verbose);
            //    var rootedPluginFiles = pluginFiles.Select(pf => fileSystem.Path.GetFullPath(pf));
            //    Output("Rooted plugin files", () => rootedPluginFiles, options.Verbose);

            //    var plugins = rootedPluginFiles.Select(rpf => (
            //        RootedPath: fileSystem.Path.GetDirectoryName(rpf),
            //        PluginFileName: fileSystem.Path.GetFileName(rpf),
            //        ManifestFileName: fileSystem.Path.GetFileNameWithoutExtension(rpf) + ".plugin-manifest.xml"));
            //    Output("Plugin info", () => plugins.Select(x => $"[{x.RootedPath},{x.PluginFileName},{x.ManifestFileName}]"), options.Verbose);

            //    var manifestHandler = new ManifestHandler(fileSystem);
            //    var assemblies = new List<IAssembly>();
            //    foreach (var plugin in plugins)
            //    {
            //        var manifestPathfile = fileSystem.Path.Combine(plugin.RootedPath, plugin.ManifestFileName);
            //        if (fileSystem.File.Exists(manifestPathfile))
            //        {
            //            var manifest = manifestHandler.Read(manifestPathfile);
            //            LoadManifestFiles(manifest, plugin.RootedPath, options.Verbose);
            //        }

            //        var pluginPathfile = fileSystem.Path.Combine(plugin.RootedPath, plugin.PluginFileName);
            //        var loadedPlugin = assemblyFactory.LoadFile(pluginPathfile);
            //        assemblies.Add(loadedPlugin);
            //    }

            //    pluginHandler.Load(assemblies);
            //}

            if (options.TheirPath == "")
            {
                var filesDocument = LogicFactory.CreateForPath(
                    fileSystem,
                    myFilesRoot!,
                    pluginHandler).AsXDocument();

                if (options.Processors != string.Empty)
                {
                    //  We don't handle paths and files with space in them. Yet.
                    var processorFiles = options.Processors.Split(" ");
                    Output("Processor files", () => processorFiles, options.Verbose);
                    var rootedProcessorFiles = processorFiles.Select(pf => fileSystem.Path.GetFullPath(pf));
                    Output("Rooted processor files", () => rootedProcessorFiles, options.Verbose);

                    var processors = rootedProcessorFiles.Select(rpf => (
                        RootedPath: fileSystem.Path.GetDirectoryName(rpf),
                        PluginFileName: fileSystem.Path.GetFileName(rpf)));
                    Output("Processors info", () => processors.Select(x => $"[{x.RootedPath},{x.PluginFileName}]"), options.Verbose);

                    foreach (var processor in processors)
                    {
                        // TODO:OF:Execute.
                        ExecuteProcessor(processor, filesDocument, options);
                    }
                }

                OutputResult(filesDocument, outputter);
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

        private static XElement ShallowClone(XElement source)
        {
            var target = new XElement(source.Name);
            foreach (var attribute in source.Attributes())
            {
                target.Add(new XAttribute(attribute));
            }
            return target;
        }

        private static IEnumerable<(XDocument DocumentToExport, XElement OriginalElement)> AllFiles(XDocument sourceDocument)
        {
            foreach (var dir in sourceDocument.Root?.Nodes() ?? Enumerable.Empty<XNode>()) // TODO:OF:Remove ?? as we should know we have contents.
            {
                var dirElement = (XElement)dir;
                var dirPath = dirElement.Attribute("path");
                foreach (var fileNode in dirElement.Nodes() ?? new XElement[0])
                {
                    var targetRootElement = ShallowClone(sourceDocument.Root!);
                    var targetFileElement = ShallowClone((XElement)fileNode);
                    targetRootElement.Add(targetFileElement);

                    var xdoc = new XDocument(targetRootElement);
                    yield return (DocumentToExport: xdoc, OriginalElement: dirElement);
                }
            }
        }

        private static string Quote(string txt)
        {
            // Ugly trick to replace " and \. It with not work as soon as the path or filename contains the constants.
            const string QuoteToken = "[QUOTE]";
            const string BackslashToken = "[BACKSLASH]";
            const string Quote = "\"";
            const string Backslash = "\\";
            const string BackslashQuote = Backslash + Quote;
            const string BackslashX2 = Backslash + Backslash;

            var a = txt.Replace(Quote, QuoteToken);
            var b = a.Replace(Backslash, BackslashToken);
            return Quote +
                b.Replace(QuoteToken, BackslashQuote).Replace(BackslashToken, BackslashX2) +
                Quote;
        }

        private string ExecuteExe((string RootedPath, string PluginFileName) processor, Options options, string argument)
        {
            var process = new Process();
            process.StartInfo.FileName = fileSystem.Path.Combine(processor.RootedPath, processor.PluginFileName);
            process.StartInfo.Arguments = Quote(argument);

            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = false;

            //process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            // Just Console.WriteLine it.
            process.ErrorDataReceived += ErrorDataReceived;

            Output("Processor", () => $"Starting {processor}", options.Verbose);

            process.Start();

            process.BeginErrorReadLine();

            string result;
            try
            {
                result = process.StandardOutput.ReadToEnd() ?? string.Empty;
            }
            finally
            {
                if (process.HasExited == false)
                    process.Kill();
            }

            Output("Success", () => result);

            return result;

            void ErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                //  By some reason I have yet to grasp this event handler is called with `e.Datat==null`
                //  when a line feecd andor carriage return is called. Not an error.
                if (e?.Data is not null)
                {
                    Output("Processor, error", () => "* " + nameof(ErrorDataReceived));
                    Output("Processor, error, sender", () => sender?.ToString() ?? string.Empty);
                    Output("Processor, error, data", () => e.Data?.ToString() ?? string.Empty);
                }
            }
        }

        private void ExecuteProcessor((string RootedPath, string PluginFileName) processor, XDocument filesDocument, Options options)
        {
            var exports = AllFiles(filesDocument);

            foreach (var export in exports)
            {
                ExecuteExe(processor, options, export.DocumentToExport.ToString());
            }
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
