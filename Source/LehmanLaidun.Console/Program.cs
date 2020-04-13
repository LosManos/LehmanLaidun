using System;
using System.Collections.Generic;
using LehmanLaidun.FileSystem;
using C = System.Console;

namespace LehmanLaidun.Console
{
    enum ReturnValues
    {
        Success = 0,
        NoInput = 1,
        InvalidMyDirectory = 2,
        InvalidTheirsDirectory = 3
    }

    class Program
    {
        static int Main(string[] args)
        {
            if((args??new string[0]).Length != 2)
            {
                OutputMessage();
                return (int)ReturnValues.NoInput;
            }

            if( false == TryDirectoryExists(args[0],out string myFilesRoot))
            {
                return (int)ReturnValues.InvalidMyDirectory;
            }

            if( false == TryDirectoryExists(args[1], out string theirFilesRoot))
            {
                return (int)ReturnValues.InvalidTheirsDirectory;
            }

            var myFiles = LogicFactory.CreateForPath(new System.IO.Abstractions.FileSystem(), myFilesRoot).AsXDocument();
            var theirFiles = LogicFactory.CreateForPath(new System.IO.Abstractions.FileSystem(), theirFilesRoot).AsXDocument();

            var differences = Logic.CompareXml(myFiles, theirFiles, new[] { "name", "length" });

            //TODO:Make other output if differences.Result == true;

            OutputResult(differences);

            return (int)ReturnValues.Success;
        }

        private static void OutputMessage()
        {
            C.WriteLine("* LehmanLaidun.Console *");
            C.WriteLine("This program compares two directory trees and returns the differences.");
            C.WriteLine();
            C.WriteLine("Examples of usage:");
            C.WriteLine("dotnet LehmanLaidun.Console.dll \"C:\\MyMusic\" E:\\");
            C.WriteLine("Or:");
            C.WriteLine(@".\bin\Debug\netcoreapp3.1\LehmanLaidun.Console.exe .\Data\MyDrive\ .\Data\TheirDrive\");
        }

        private static void OutputResult((bool Result, IEnumerable<Difference> Differences) diff)
        {
            foreach ( var d in diff.Differences)
            {
                if( d.FirstXPath != null)
                {
                    C.WriteLine($"Found only in first:{d.FirstXPath}.");
                }
                if( d.SecondXPath != null)
                {
                    C.WriteLine($"Found only in second:{d.SecondXPath}.");
                }
            }
        }

        private static bool TryDirectoryExists(string possibleDirectory, out string validDirectory)
        {
            if(System.IO.Directory.Exists(possibleDirectory))
            {
                validDirectory = possibleDirectory;
                return true;
            }
            validDirectory = null;
            return false;
        }
    }
}
