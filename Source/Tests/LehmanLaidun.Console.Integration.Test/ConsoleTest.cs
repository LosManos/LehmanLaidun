using FluentAssertions;
using LehmanLaidun.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace LehmanLaidun.Console.Integration.Test
{
    [TestClass]
    public class ConsoleTest
    {
        [TestMethod]
        public void CompareTwoFolders()
        {
            var expectedOutput =
@"Found only in first:/root/directory[@name='Summer 2018']/file[@name='cat.jpg'].
Found only in first:/root/directory[@name='Summer 2018']/file[@name='museum.jpg'].
Found only in second:/root/directory[@name='Summer 2018']/file[@name='Birds at pier.jfif'].
".NormaliseLineEndings().Split("\r\n");

            var arguments = 
                "--mypath=" + Path.Combine(TheFileSystem.FindDataPath(), MethodBase.GetCurrentMethod()!.Name, "MyDrive") + 
                " " + 
                "--theirpath=" + Path.Combine(TheFileSystem.FindDataPath(), MethodBase.GetCurrentMethod()!.Name, "TheirDrive");

            //  Act.
            var output = TheFileSystem.Execute(arguments);

            //  Assert.
            output.NormaliseLineEndings().Split("\r\n").OrderBy(s=>s)
                .Should().BeEquivalentTo(expectedOutput, options=> options.WithoutStrictOrdering());
        }

        [TestMethod]
        public void OutputOneFolderAsXml()
        {
            //  As the output from the console contains information which is hard to compare between environments, 
            //  like path and lastWriteTime, we choose to compare only simpler attributes.
            var expectedOutput =
                @"<root >
                  <directory name=""Summer 2018"">
                    <file name=""Birds at pier.jfif"" />
                    <file name=""field.jpg"" />
                    <file name=""swans.jpg"" />
                  </directory>
                </root>";

            var arguments = 
                "--mypath=" + Path.Combine(TheFileSystem.FindDataPath(), MethodBase.GetCurrentMethod()!.Name, "ADrive") + 
                " " + 
                "--ox";

            //  Act.
            var output = TheFileSystem.Execute(arguments);

            //  Assert.
            //  It is hard to compare the string output as it contains environment information, 
            //  like path, so we convert everything to Xml and compare just some attributes;
            //  but we do also compare the whole structure.
            var result = Logic.CompareXml(
                XDocument.Parse(expectedOutput),
                XDocument.Parse(output),
                new[] { "name", "length" });
            result.Result.Should().BeTrue();
        }
    }
}
