using FluentAssertions;
using LehmanLaidun.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace LehmanLaidun.FileSystem.Integration.Test
{
    [TestClass]
    public class PluginHandlerTest
    {
        private readonly System.IO.Abstractions.FileSystem fileSystem;
        private readonly PathWrapper pathWrapper;

        public PluginHandlerTest()
        {
            fileSystem = new System.IO.Abstractions.FileSystem();
            pathWrapper = new PathWrapper(fileSystem);
        }

        [TestMethod]
        [TestCategory("LocalOnly")]
        public void CanLoadAndExecute()
        {
            var plugins = new[] {
                new {Name = "PluginOne", Framework = "netstandard2.0" },
                new{Name = "PluginTwo", Framework = "net5.0"},
            };
            var pluginPathFiles = plugins.Select(plugin =>
                pathWrapper.Combine(
                    PathToPlugins(),
                    plugin.Name,
                    "bin",
                    "Debug",
                    plugin.Framework,
                    plugin.Name + ".dll")
            );

            var assemblies = pluginPathFiles.Select(pluginPathFile => Assembly.LoadFile(pluginPathFile));

            var sut = PluginHandler.Create();

            //  Act.
            sut.Load(assemblies);
            var res = sut.Execute("a");

            //  Assert.
            res
                .Select(pr => new { pr.Name, Result = pr.Result.ToString() })
                .Should()
                .BeEquivalentTo(
                    new[]{
                        new { Name = "Plugin one", Result = "<data plugin=\"Plugin one\">a</data>" },
                        new { Name = "Plugin two", Result = "<data plugin=\"Plugin two\">a</data>" },
                    }
                );
        }

        private string PathToPlugins()
        {
            var assemblyLocation = typeof(PluginHandler).Assembly.Location;
            var assemblyDirectory = pathWrapper.GetDirectoryName(assemblyLocation);
            var root = pathWrapper.Combine(assemblyDirectory, "..", "..", "..", "..", "..", "Plugins");
            return root;
        }
    }
}
