using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

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


            var arguments = Path.Combine(FindDataPath(), "MyDrive") + " " + Path.Combine(FindDataPath(), "TheirDrive");
            string output;

            //  Act.
            using (var p = new Process())
            {
                p.StartInfo.FileName = FindConsoleExecutable();
                p.StartInfo.WorkingDirectory = FindTesteePrjPath();
                p.StartInfo.Arguments = arguments;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                if (p.ExitCode != 0)
                {
                    output = output + Environment.NewLine + $"ExitCode=[{p.ExitCode}]{Environment.NewLine}{output}";
                }
            }

            //  Assert.
            output.NormaliseLineEndings()
                .Should().Be(expectedOutput);
        }

        private static string FindConsoleBinPath()
        {
#if DEBUG
            var releaseDebugFolder = "Debug";
#endif
#if RELEASE
            var releaseDebugFolder = "Release";
#endif
            var exeDirectory = Path.Combine(FindTesteePrjPath(), "bin", releaseDebugFolder, "netcoreapp3.1");
            return exeDirectory;
        }

        private static string FindConsoleExecutable()
        {
            var fileEnding = Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => ".exe",
                PlatformID.Win32Windows => ".exe",  //  Not tested.
                PlatformID.Unix => ".Console",
                PlatformID.MacOSX => ".Console",    // Not tested.
                PlatformID.Win32S => throw new NotImplementedException(),
                PlatformID.WinCE => throw new NotImplementedException(),
                PlatformID.Xbox => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
            var exeFileName = Directory.GetFiles(FindConsoleBinPath())
                .First(x => x.EndsWith(fileEnding));   // Drill down and hope for the dotnet version and file name to as anticipated.
            return exeFileName;
        }

        private static string FindDataPath()
        {
            var exeDirectory = Path.Combine(FindTesteePrjPath(), "Data");
            return exeDirectory;
        }

        private static string FindTesteePrjPath()
        {
            var currentDirectory = Directory.GetCurrentDirectory(); // Get the test directory.
            var currentRoot = Path.Combine(currentDirectory, "..", "..", "..", "..", "..");    // Go to the closes common parent.
            var slnPath = Path.Combine(currentRoot, "LehmanLaidun.Console");
            return slnPath;
        }
    }
}
