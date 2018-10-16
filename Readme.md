# Readme for LehmanLaidun

LehmanLaidun.FileSystem.Logic logic = LehmanLaidun.FileSystem.LogicFactory.CreateForPath(@"D:\");
System.Collections.Generic.IEnumerable<LehmanLaidun.FileSystem.FileItem> files = logic.AsEnumerableFiles();
System.Xml.Linq.XDocument xml = logic.AsXDocument();

(bool Result, IEnumerable<Difference> Differences) diffs = CompareXml( (XDocument) doc1, (XDocument) doc2 );
