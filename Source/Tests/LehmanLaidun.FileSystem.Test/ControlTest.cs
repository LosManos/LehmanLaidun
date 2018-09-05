using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;

namespace LehmanLaidun.FileSystem.Test
{
	[TestClass]
	public class ControlTest
	{
		[TestMethod]
		public void CanTraverseTree()
		{
            const string Path = @"c:\images";
            var paths = new[] {
                @"c:\images\20180924\image1.jpg",
                @"c:\images\20180922\image3.jpg",
                @"c:\images\20180924\image2.jpg" };

            var mockedFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            //{
            //    { @"c:\images\20180924\image1.jpg", new MockFileData("a") },
            //    { @"c:\images\20180922\image3.jpg", new MockFileData("a") },
            //    { @"c:\images\20180924\image2.jpg", new MockFileData("a") }
            //});
            paths.ToList().ForEach(p => mockedFileSystem.AddFile(p, new MockFileData("a")));

            var sut = Control.CreateForPath(Path)
                .Inject(mockedFileSystem);

            var res = sut.AsEnumerableFiles();

            res.Count().Should().Be(3, "Everything should be accounted for.");
            res.Select(r=>System.IO.Path.Combine(r.Path,r.Name))
                .Should().Contain(paths, 
                "Every file should be there in any order.");
   		}
	}
}
