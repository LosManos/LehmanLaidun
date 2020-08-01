using FluentAssertions;
using LehmanLaidun.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace LehmanLaidun.Console.Test
{
    [TestClass]
    public class ConsoleTest
    {
        [TestMethod]
        public void CompareTwoFolders()
        {
            var expectedOutput =
@"Found only in first:/root/directory[@name='Summer 2018']/file[@name='cat.jpg' and @length='9452'].
Found only in first:/root/directory[@name='Summer 2018']/file[@name='museum.jpg' and @length='6618'].
Found only in second:/root/directory[@name='Summer 2018']/file[@name='Birds at pier.jfif' and @length='5327'].
".NormaliseLineEndings();

            var arguments = Path.Combine(TheFileSystem.FindDataPath(), MethodBase.GetCurrentMethod()!.Name, "MyDrive") + 
                " " + 
                Path.Combine(TheFileSystem.FindDataPath(), MethodBase.GetCurrentMethod()!.Name, "TheirDrive");

            //  Act.
            var output = TheFileSystem.Execute(arguments);

            //  Assert.
            output.NormaliseLineEndings()
                .Should().Be(expectedOutput);
        }

        [TestMethod]
        public void OutputOneFolderAsXml()
        {
            //  As the output from the console contains information which is hard to compare between environments, 
            //  like path and lastWriteTime, we choose to compare only simpler attributes.
            var expectedOutput =
                @"<root >
                  <directory name=""Summer 2018"">
                    <file name=""Birds at pier.jfif"" length=""5327"" />
                    <file name=""field.jpg"" length=""6522"" />
                    <file name=""swans.jpg"" length=""8140"" />
                  </directory>
                </root>";

            var arguments = Path.Combine(TheFileSystem.FindDataPath(), MethodBase.GetCurrentMethod()!.Name, "ADrive") + 
                " " + 
                "-ox";

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
