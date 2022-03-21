using CompulsoryCow.AssemblyAbstractions;
using FluentAssertions;
using LehmanLaidun.FileSystem;
using LehmanLaidun.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace LehmanLaidun.Console.Unit.Test
{
    [TestClass]
    public class ProgramTest
    {
        private string Root = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"c:" :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/c" :
                throw new System.Exception($"Unkown operating sytem {RuntimeInformation.OSDescription}");

        [TestMethod]
        public void ProgramShouldReturnNoInputForNoImput()
        {
            var args = new string[] { };

            //  Act.
            var res = (ReturnValues)Program.Main(args);

            //  Assert.
            res.Should().Be(ReturnValues.NoInput);
        }

        [TestMethod]
        public void ProgramShouldReturnInvalidMyDirectoryForInvalidMyDirectory()
        {
            var fileSystem = SetupFilesystem(
                (Root, "mydata", "a file", "file content")
            );
            var sut = new ProgramImpl(
                //SetupPlugins().Object,
                fileSystem,
                //new AssemblyFactory(),
                SetupOutputter(out _).Object
            );

            //  Act.
            var res = sut.Execute(new Options
            {
                MyPath = System.IO.Path.Combine(Root, "notmydata")
            });

            //  Assert.
            res.Should().Be(ReturnValues.InvalidMyDirectory);
        }

        [TestMethod]
        public void ProgramShouldReturnXmlForMyPath()
        {
            var mockedFileSystem = SetupFilesystem(
                new[] { (Root, "data", "tempfile", "file content") }
            );

            var sut = new ProgramImpl(
                //SetupPlugins().Object,
                mockedFileSystem,
                //new AssemblyFactory(),
                SetupOutputter(out List<string> outputs).Object
            );

            //  Act.
            var res = sut.Execute(new Options
            {
                MyPath = System.IO.Path.Combine(Root, "data")
            });

            //  Assert.
            res.Should().Be(ReturnValues.Success);

            outputs.Count.Should().Be(1);

            var xdoc = XDocument.Parse(outputs[0]);
            xdoc.Should().NotBeNull();
        }

        [TestMethod]
        public void ProgramShouldReturnDiffForComparingTwoFolders()
        {
            var mockedFileSystem = SetupFilesystem(
                (Root, "mydata", "afile", "file content"),
                (Root, "theirdata", "bfile", "file content")
            );

            var sut = new ProgramImpl(
                //SetupPlugins().Object,
                mockedFileSystem,
                //new AssemblyFactory(),
                SetupOutputter(out List<string> outputs).Object
            );

            //  Act.
            var res = sut.Execute(new Options
            {
                MyPath = System.IO.Path.Combine(Root, "mydata"),
                TheirPath = System.IO.Path.Combine(Root, "theirdata"),
            });

            //  Assert.
            res.Should().Be(ReturnValues.Success);

            outputs.Count.Should().Be(2);

            outputs[0].Should().StartWith("Found only in first");
            outputs[1].Should().StartWith("Found only in second");
        }

        [TestMethod]
        public void ProgramShouldReturnInvalidForInvalidTheirsDirectory()
        {
            var mockedFileSystem = SetupFilesystem(
                (Root, "mydata", "afile", "file content"),
                (Root, "theirdata", "bfile", "file content")
            );

            var sut = new ProgramImpl(
                //SetupPlugins().Object,
                mockedFileSystem,
                //new AssemblyFactory(),
                SetupOutputter(out _).Object
            );

            //  Act.
            var res = sut.Execute(new Options
            {
                MyPath = System.IO.Path.Combine(Root, "mydata"),
                TheirPath = System.IO.Path.Combine(Root, "nottheirdata"),
            });

            //  Assert.
            res.Should().Be(ReturnValues.InvalidTheirsDirectory);
        }

        [TestMethod]
        public void ShouldExecuteProcessorFile()
        {
            var file = new FakeFile(Root, "mypath", "myfile", "file content");
            var processorFile = new FakeFile(Root, "", "processor.exe", "file content");

            var sut = new ProgramImpl(
                SetupFilesystem(
                    file,
                    processorFile
                ),
                SetupOutputter(out _).Object
            );

            //  Act.
            var res = sut.Execute(new Options
            {
                MyPath = System.IO.Path.Combine(Root, file.Path),
                //PluginFiles = processorFile.PathFile,
                Processors = processorFile.PathFile
            });

            //  Assert.
            res.Should().Be(ReturnValues.Success);

            false.Should().BeTrue("TODO:OF:TBA");
        }

        private class FakeFile
        {
            internal string Root { get; }
            internal string Path { get; }
            internal string Name { get; }
            internal string Content { get; }
            internal string PathFile => System.IO.Path.Combine(Root, Path, Name);
            internal FakeFile(string root, string path, string name, string content)
            {
                Root = root;
                Path = path;
                Name = name;
                Content = content;
            }
        }

        private static Mock<IAssemblyFactory> SetupAssembly(
            string pluginPathfile,
            Mock<IAssembly> mockAssembly)
        {
            var mockFactory = new Mock<IAssemblyFactory>(MockBehavior.Strict);
            mockFactory.Setup(m => m.LoadFile(pluginPathfile))
                .Returns(mockAssembly.Object);

            return mockFactory;
        }

        private static MockFileSystem SetupFilesystem(params FakeFile[] files)
        {
            return SetupFilesystem(files.Select(f => (root: f.Root, path: f.Path, filename: f.Name, content: f.Content)).ToArray());
        }

        private static MockFileSystem SetupFilesystem(params (string root, string path, string filename, string content)[] fileData)
        {
            var filesystem = new System.IO.Abstractions.FileSystem();
            var mockedFileSystem = new MockFileSystem();
            foreach (var file in fileData)
            {
                mockedFileSystem.AddFile(
                    filesystem.Path.Join(file.root, file.path, file.filename),
                    new MockFileData(file.content)
                );
            }
            return mockedFileSystem;
        }

        private static Mock<IOutputter> SetupOutputter(out List<string> outputs)
        {
            outputs = new List<string>();
            var forAnonymousLambda = outputs;

            var m = new Mock<IOutputter>(MockBehavior.Strict);
            m.Setup(m =>
                m.WriteLine(It.IsAny<string>()))
                .Callback((string message) => { forAnonymousLambda.Add(message); });
            return m;
        }

        private static Mock<IPluginHandler> SetupPlugins(
            Mock<IAssembly>? mockAssembly = null
        )
        {
            var mock = new Mock<IPluginHandler>(MockBehavior.Strict);

            mock.Setup(m =>
                m.Execute(It.IsAny<string>()))
                .Returns(new ParseResult[] { });

            if (mockAssembly != null)
            {
                mock.Setup(m =>
                    m.Load(It.Is<List<IAssembly>>(
                        ass => ass.Count == 1
                        && ass[0].FullName == "MyFullName"))
                );
            }

            return mock;
        }
    }
}
