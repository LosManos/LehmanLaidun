# Readme for LehmanLaidun

## Example of usage.

### Get info files and folder info from a folder structure.

LehmanLaidun.FileSystem.Logic logic = LehmanLaidun.FileSystem.LogicFactory.CreateForPath(@"D:\");
System.Collections.Generic.IEnumerable<LehmanLaidun.FileSystem.FileItem> files = logic.AsEnumerableFiles();
System.Xml.Linq.XDocument xml = logic.AsXDocument();

### Compare 2 xml files.

var doc1 = LogicFactory.CreateForPath(@"C:\MyMusic").AsXDocument();
var doc2 = LogicFactory.CreateForPath(@"D:\MyUsb").AsXDocument();

(bool Result, IEnumerable<Difference> Differences) comparison = CompareXml( (XDocument) doc1, (XDocument) doc2 );

Console.WriteLine( $"Folder structure is equal:{comparison.Result}."); // "Folder structure is equal:False".
foreach( var diff in comparison.Differences ){
	Console.WriteLine( ... )
}