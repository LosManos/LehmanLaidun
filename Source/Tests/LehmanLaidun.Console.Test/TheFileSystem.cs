using Microsoft.VisualStudio.TestPlatform.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LehmanLaidun.Console.Test
{
    /// <summary>This class contains functionality for handling the "physical" file system when testing.
    /// </summary>
    internal class TheFileSystem
    {
        /// <summary>This method executes the console application
        /// with the parameters provided
        /// and returns the result.
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        internal static string Execute(string arguments)
        {
            using (var p = new Process())
            {
                p.StartInfo.FileName = TheFileSystem.FindConsoleExecutable();
                p.StartInfo.WorkingDirectory = TheFileSystem.FindTesteePrjPath();
                p.StartInfo.Arguments = arguments;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                if (p.ExitCode != 0)
                {
                    output = output + Environment.NewLine + $"ExitCode=[{p.ExitCode}]{Environment.NewLine}{output}";
                }
                return output.NormaliseLineEndings();
            }
        }

        /// <summary>This method returns the full path to the Data folder, the folder where test data is kept.
        /// </summary>
        /// <returns></returns>
        internal static string FindDataPath()
        {
            var exeDirectory = Path.Combine(FindTesteePrjPath(), "Data");
            return exeDirectory;
        }

        /// <summary>This method returns the binary path, the one edning with "netcoreapp...".
        /// This method will return the wrong result when the dotnet version is updated.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>This method returns the name of the executable.
        /// It can handle several operating systems and throws an exception
        /// if un unknown operating system is encountered.
        /// </summary>
        /// <returns></returns>
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
                .First(x => x.EndsWith(fileEnding));   // Drill down and hope for the dotnet version and file name to be as anticipated.
            return exeFileName;
        }

        /// <summary>This method returns the path of the csproj file. Not including the csproj file.
        /// </summary>
        /// <returns></returns>
        private static string FindTesteePrjPath()
        {
            var currentDirectory = Directory.GetCurrentDirectory(); // Get the test directory.
            var currentRoot = Path.Combine(currentDirectory, "..", "..", "..", "..", "..");    // Go to the closes common parent.
            var prjPath = Path.Combine(currentRoot, "LehmanLaidun.Console");
            return prjPath;
        }
    }
}
