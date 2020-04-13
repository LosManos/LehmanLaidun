using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LehmanLaidun.Console.Test
{
    [TestClass]
    public class ConsoleTest
    {
        [TestMethod]
        public void Renameme() // TODO:OF:Rename.
        {
            const string expectedOutput =
@"Found only in first:/root/directory[@name='Summer 2018']/file[@name='cat.jpg' and @length='9452'].
Found only in first:/root/directory[@name='Summer 2018']/file[@name='museum.jpg' and @length='6618'].
Found only in second:/root/directory[@name='Summer 2018']/file[@name='Birds at pier.jfif' and @length='5327'].
";
            string output;
            using (var p = new Process())
            {
                p.StartInfo.FileName = FindConsoleExe();
                p.StartInfo.WorkingDirectory = FindTesteeSlnPath();
                p.StartInfo.Arguments = @".\Data\MyDrive\ .\Data\TheirDrive\";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
            Assert.AreEqual(expectedOutput, output);
        }

        private static string FindConsoleExe()
        {
            var currentDirectory = Directory.GetCurrentDirectory(); // Get the test directory.
            var currentRoot = Path.Combine(currentDirectory, @"..\..\..\..\..");    // Go to the closes common parent.
            var exeFileName = Directory.GetFiles(Path.Combine(currentRoot, @"LehmanLaidun.Console\bin\debug\netcoreapp3.1")
                ).First(x => x.EndsWith(".exe"));   // Drill down and hope for the dotnet version and file name to as anticipated.
            return exeFileName;
        }

        private static string FindTesteeSlnPath()
        {
            var currentDirectory = Directory.GetCurrentDirectory(); // Get the test directory.
            var currentRoot = Path.Combine(currentDirectory, @"..\..\..\..\..");    // Go to the closes common parent.
            var slnPath = Path.Combine(currentRoot, "LehmanLaidun.Console");
            return slnPath;
        }
    }
}
