using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using static LehmanLaidun.FileSystem.Difference;

namespace LehmanLaidun.FileSystem
{
    public class Logic
    {
        private IFileSystem _fileSystem;

        public string Path { get; }
        
        /// <summary>This static constructor is the prefered.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static Logic Create(IFileSystem fileSystem, string path)
        {
            return new Logic(fileSystem, path);
        }

        /// <summary>This constructor takes all parameters needed to fully populate the object.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="path"></param>
        private Logic(IFileSystem fileSystem, string path)
        {
            _fileSystem = fileSystem;
            Path = path;
        }

        /// <summary>This method returns the file system's files as a list.
        /// The algorithm is depth first.
        /// The list is yielded, so <see cref="FileItem"/>s are returned continuously.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileItem> AsEnumerableFiles()
        {
            //  First take care of the files in the folder asked for.
            foreach (var file
                in _fileSystem.Directory.EnumerateFiles(Path, "*", System.IO.SearchOption.TopDirectoryOnly))
            {
                yield return FileItem.Create(Path, _fileSystem.Path.GetFileName(file));
            }
            //  Then recurse the directories.
            foreach (var directory
                in _fileSystem.Directory.EnumerateDirectories(Path, "*", System.IO.SearchOption.AllDirectories))
            {
                foreach (var file
                    in _fileSystem.Directory.EnumerateFiles(directory, "*", System.IO.SearchOption.TopDirectoryOnly))
                {
                    yield return FileItem.Create(directory, _fileSystem.Path.GetFileName(file));
                }
            }
        }
    
        /// <summary>This method returns the file system's files as an XML.
        /// </summary>
        /// <returns></returns>
        public XDocument AsXDocument()
        {
            var doc = new XDocument(new XElement("root", new XAttribute("path", Path)));
            foreach (var file in AsEnumerableFiles())
            {
                //  Remove the first part, the Path.
                var relPath = file.Path.Remove(0, Path.Length);
                var directoryNames = relPath.Trim(new[] { _fileSystem.Path.DirectorySeparatorChar }).Split(_fileSystem.Path.DirectorySeparatorChar);
                var directoryElement = doc.Root;
                for (var i = 0; i < directoryNames.Length; ++i)
                {
                    var directoryNameList = directoryNames.Take(i + 1); // The first n items.

                    directoryElement = doc.SelectDirectoryElement(directoryNameList) ?? directoryElement.AddDirectoryElement(directoryNameList.Last());

                }
                directoryElement.AddFileElement(file);
            }
            return doc;
        }

        /// <summary>This method returns the difference between two XMLs from <see cref="AsXDocument"/>.
        /// <code>
        /// var myMusic = Logic.Create( @"C:\MyMusic" ).AsXDocument();
        /// var theirMusic = Logic.Create( "E:\" );
        /// var difference = Logic.CompareXml( myMusic, theirMusic );
        /// </code>
        /// </summary>
        /// <param name="xml1"></param>
        /// <param name="xml2"></param>
        /// <returns></returns>
        public static (bool Result, IEnumerable<Difference> Differences) CompareXml(XDocument xml1, XDocument xml2)
        {
            var firstResult = Compare(xml1.Root, xml2.Root, 0, FoundOnlyIn.First);
            var secondResult = Compare(xml2.Root, xml1.Root, 0, FoundOnlyIn.Second);
            var result = firstResult.Concat(secondResult);
            return (result.Any() == false, result);
        }

        private static IEnumerable<Difference> Compare(XElement firstElement, XElement secondElement, int rowNum, FoundOnlyIn foundOnlyIn)
        {
            var diffs = new List<Difference>();

            var firstXPath = GetXPathOf(firstElement);
            var existingElements = secondElement.Document.XPathSelectElements(firstXPath);
            if( existingElements.Any() == false)
            {
                diffs.Add(Difference.Create(firstElement.ShallowCopy(), firstXPath, foundOnlyIn));
            }else if( existingElements.Count() == 1)
            {
                // OK.
            }
            foreach( var child in firstElement.Elements())
            {
                diffs.AddRange(Compare(child, secondElement, 0, foundOnlyIn));
            }
            return diffs;
        }

        private static string GetXPathOf(XElement element)
        {
            var elementPath = 
                "/" +
                element
                    .Parents()
                    .Reverse()
                    .Select(e => e.Name.LocalName + GetXPathOf(e.Attributes()))
                    .StringJoin("/");
            return elementPath;
        }

        private static string GetXPathOf(IEnumerable<XAttribute> attributes)
        {
            return 
                "[" +
                (attributes.Any() ?
                    // TODO: Escape the values.
                    attributes
                        .Select(a => $"@{a.Name}='{a.Value}'")
                        .StringJoin(" and ") :
                    "not(@*)" 
                )+
                "]";

        }
        
        private static bool ElementsAreEqual(XElement element1, XElement element2)
        {
            return ElementNamesAreEqual(element1, element2) &&
                ElementAttributesAreEqual(element1, element2);
        }

        private static bool ElementAttributesAreEqual(XElement element1, XElement element2)
        {
            throw new NotImplementedException();
        }

        private static bool ElementNamesAreEqual(XElement element1, XElement element2)
        {
            return element1.Name == element2.Name;
        }

        /// <summary>This recursive method sorts an xml tree.
        /// Sorting order is element and attribute names. The order of the attributes is not interesting
        /// and by that means that an element E with attributes A and B is equal in sort order
        /// as an element E with attributes B and a.
        /// The algorithm recurses the tree and creates copies of the elements from furthest way and back to the root.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static XElement Sort(XElement element)
        {
            // For all the element's children, recursively call Sort or create a shallow copy if there are no children.
            // In the end we have a copy of the tree we're at.
            var children =
                element.Elements()
                    .Select(e => e.Elements().Any() ?
                        Sort(e) :
                        e.ShallowCopy());

            // Copy the incoming element and add the children we have created earlier.
            return element
                .ShallowCopy()
                .AddElements(
                    children.OrderBy(e => e.Name.LocalName).ThenBy(e => SortableAttributes(e)));
        }

        /// <summary>This method sorts the XML tree
        /// by the name of the elements and then the name of the attributes.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        private static XDocument SortXml(XDocument xml)
        {
            return new XDocument(Sort(xml.Root));
        }

        /// <summary>This method returns a string representing all attribute names.
        /// Together with the element name one can then sort the elements on name and attributes.
        /// 
        /// NOTE: The implementation is not totally correct. A character that cannot be used as an attribute is used as delimiter
        /// but that does not work if the attributes has characters sorted both before and after than the delimiter.
        /// For instance, 0x9, 0xA and 0xD are probably sorted before the delimiter, &gt; ,which is somewhere around 34(decimal).
        /// Normal letters (>=64dec) are, I guess, sorted after the delimiter.
        /// <see cref="https://www.w3.org/TR/xml/#charsets"/>
        /// But for now it works.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static string SortableAttributes(XElement element)
        {
            var attributes = element.Attributes().OrderBy(a => a.Name.LocalName);
            return string.Join(">", attributes);
        }

        #region Helper methods for making unit testing possible.

        /// <summary>This method is intended to be used by unit tests only.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        internal static XDocument UT_SortXml(XDocument xml)
        {
            return SortXml(xml);
        }

        #endregion
    }
}
