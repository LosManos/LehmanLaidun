using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Abstractions.TestingHelpers;

namespace LehmanLaidun.Console.Unit.Test
{
    [TestClass]
    public class ProgramTest
    {
        [TestMethod]
        public void ProgramMainShouldReturnInvalidMyDirectoryForNotExistingMyPath()
        {
            var mockedFileSystem = new MockFileSystem();

            Program.fileSystem = mockedFileSystem;

            var args = new string[] { };

            //  Act.
            var res = (ReturnValues)Program.Main(args);

            //  Assert.
            res.Should().Be(ReturnValues.InvalidMyDirectory);
        }

        //[TestMethod]
        //public void asdf()
        //{
        //    var Pathfile = System.IO.Path.Join(@"C:\data\:", "tempfile");
        //    var mockedFile = new MockFileData("file content");

        //    var mockedFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(){
        //        {Pathfile, mockedFile }
        //    });
        //    Program.fileSystem = mockedFileSystem;

        //    var args = new[] { //"mypath", "asdf", 
        //    "--mypath", @"C:\data\", 
        //    //"mypath asdf", 
        //    //"--mypath asdf"
        //    };

        //    //  Act.
        //    var res = (ReturnValues)Program.Main(args);

        //    //  Assert.
        //    res.Should().Be(ReturnValues.Success);

        //    1.Should().Be(2, "TBA, test we are really calling what should be called");
        //}
    }
}
