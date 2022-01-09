// See https://aka.ms/new-console-template for more information
//using System.Reflection;
//Console.WriteLine("Hello, World! [" + Assembly.GetExecutingAssembly().FullName + "]");
//Console.WriteLine(string.Join(",", args ?? new string[0]));

using System.Xml.Linq;

var argument = string.Join(",", args ?? new string[0]);

var doc = XDocument.Parse(argument);
var rootElement = doc.Root;
var dirElement = rootElement.Element("directory");
var fileElement = rootElement.Element("file");
var nameAttribute = fileElement?.Attribute("name");

if (nameAttribute != null)
{
    Console.WriteLine($"<result><file newName='{"newname-" + nameAttribute.Value + ""}'/></result>");
}
else
{
    var x = (dirElement == null ? "dir:null" : $"dir:val") +
        (fileElement == null ? "file:null" : $"file:val") +
        (nameAttribute == null ? "att:null" : "att:val");
    Console.WriteLine($"<result><file newName='{x}'/></result>");
}
