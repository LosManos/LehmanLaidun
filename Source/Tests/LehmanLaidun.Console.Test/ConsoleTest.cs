using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

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

            var arguments = Path.Combine(TheFileSystem.FindDataPath(), "MyDrive") + " " + Path.Combine(TheFileSystem.FindDataPath(), "TheirDrive");

            //  Act.
            var output = TheFileSystem.Execute(arguments);

            //  Assert.
            output.NormaliseLineEndings()
                .Should().Be(expectedOutput);
        }
    }
}
