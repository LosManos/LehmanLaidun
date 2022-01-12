using System.Xml.Linq;

// Convert the arguments array to one long string.
// Well... it is comma separated so I guess
// the code needs One argument i alla fall.
var argument = string.Join(",", args ?? new string[0]);

var doc = XDocument.Parse(argument);
var rootElement = doc.Root;
var dirElement = rootElement?.Element("directory");
var fileElement = rootElement?.Element("file");
var nameAttribute = fileElement?.Attribute("name");

if (nameAttribute != null)
{
    var fileName = Path.GetFileNameWithoutExtension(nameAttribute.Value);
    var fileSuffix = Path.GetExtension(nameAttribute.Value).TrimStart('.');
    var returnDocument = XDocument.Parse(
        @$"
<result>
    <SimpleProcessor name=""{fileName}"" suffix =""{ fileSuffix}""/>
</result>
");
    Console.WriteLine(returnDocument.ToString());
}
else
{
    var x = $"dir:{dirElement?.Value ?? "null"}" +
        $"file:{fileElement?.Value ?? "null"}" +
        $"att:{nameAttribute?.Value ?? "null"}";
    Console.WriteLine(
        @$"
<result>
    <SimpleProcessor error='{x}'/>
</result>");
}
