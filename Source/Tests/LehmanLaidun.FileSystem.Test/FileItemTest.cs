using FluentAssertions;
using LehmanLaidun.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;

namespace LehmanLaidun.FileSystem.Test
{
    [TestClass]
	public partial class FileItemTest
	{
        [DataTestMethod]
        [DynamicData(nameof(TestData))]
        public void ShouldCreateWithAllBasicData(string path, string filename, string filecontent, DateTime creationDatetime)
		{
            //  #   Arrange.
            var Pathfile = System.IO.Path.Join(path, filename);
            var mockedFile = new MockFileData(filecontent)
            {
                CreationTime = creationDatetime
            };
            var mockedFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(){
                {Pathfile, mockedFile }
            });

            var mockedPluginHandler = new Mock<IPluginHandler>(MockBehavior.Strict);
            mockedPluginHandler.Setup(m => m.Execute(Pathfile))
                .Returns( new ParseResult[0]);  // No plugin installed so Execute returns nothing.

            //  #   Act.
            var res = FileItem.Create(mockedFileSystem, Pathfile, mockedPluginHandler.Object);

            //  #   Assert.
            res.Path.Should().Be(path);
            res.Name.Should().Be(filename);
            res.Data?.Any().Should().BeFalse();
        }
	}
}
