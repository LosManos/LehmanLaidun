// See https://aka.ms/new-console-template for more information
using System.Reflection;

Console.WriteLine("Hello, World! [" + Assembly.GetExecutingAssembly().FullName + "]");

Console.WriteLine(string.Join(",", args ?? new string[0]));
