using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;

namespace LehmanLaidun.FileSystem.Test
{
    [TestClass]
	public partial class FileItemTest
	{
        [DataTestMethod]
        [DynamicData(nameof(TestData))]
        public void ShouldCreateWithAllData(string path, string filename, string filecontent, DateTime creationDatetime)
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

            //  #   Act.
            var res = FileItem.Create(mockedFileSystem, Pathfile);

            //  #   Assert.
            res.Path.Should().Be(path);
            res.Name.Should().Be(filename);
            res.Length.Should().Be(filecontent.Length);
        }
	}
}
