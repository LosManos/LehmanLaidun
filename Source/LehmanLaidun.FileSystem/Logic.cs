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
        /// <summary>This is the name of the attribute that represents the length of the file.
        /// </summary>
        public static readonly string AttributeNameLength = "length";

        /// <summary>This is the name of the attribute that represents the nameo of a directory or file.
        /// </summary>
        public static readonly string AttributeNameName = "name";

        /// <summary>This is the name of the attribute that represent the path of thej root..
        /// </summary>
        public static readonly string AttributeNamePath = "path";

        /// <summary>This is the name of an element that represents a directory.
        /// </summary>
        public static readonly string ElementNameDirectory = "directory";

        /// <summary>This is the name of an element that represents a file.
        /// </summary>
        public static readonly string ElementNameFile = "file";

        /// <summary>This is the name of the root element.
        /// </summary>
        public static readonly string ElementNameRoot = "root";

        private IFileSystem _fileSystem;

        public string Path { get; }

        public class Rule
        {
            public delegate bool ComparerDelegate(
                XElement FirstElement,
                XElement SecondElement
            );

            public IEnumerable<ComparerDelegate> Comparers { get; }
            public string RuleName { get; }

            public static Rule Create(string ruleName, ComparerDelegate comparer)
            {
                return Create(ruleName, new[] { comparer });
            }

            public static Rule Create(string ruleName, IEnumerable<ComparerDelegate> comparers)
            {
                return new Rule(ruleName, comparers);
            }

            private Rule(string ruleName, IEnumerable<ComparerDelegate> comparers)
            {
                RuleName = ruleName;
                Comparers = comparers;
            }
        }

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
            foreach (var pathfile
                in _fileSystem.Directory.EnumerateFiles(Path, "*", System.IO.SearchOption.TopDirectoryOnly))
            {
                yield return FileItem.Create(_fileSystem, pathfile);
            }

            //  Then recurse the directories.
            foreach (var directory
                in _fileSystem.Directory.EnumerateDirectories(Path, "*", System.IO.SearchOption.AllDirectories))
            {
                foreach (var pathfile
                    in _fileSystem.Directory.EnumerateFiles(directory, "*", System.IO.SearchOption.TopDirectoryOnly))
                {
                    yield return FileItem.Create(_fileSystem, pathfile);
                }
            }
        }

        /// <summary>This method returns the file system's files as an XML.
        /// </summary>
        /// <returns></returns>
        public XDocument AsXDocument()
        {
            var doc = new XDocument(new XElement(ElementNameRoot, new XAttribute("path", Path)));
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

        /// <summary>This method finds all duplicate elements
        /// (elements are considered equal if their names and all attributes are equal)
        /// and returns them as a list.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static IEnumerable<Duplicate> FindDuplicates(XDocument doc)
        {
            var fd = ListOfElementsWithXpaths(doc.Root);
            var lst = fd.Where(f => f.Value.Count() >= 2);
            return lst.Select(f => Duplicate.Create(f.Value.First().ShallowCopy(), f.Value.Select(g => GetXPathOf(g))));
        }

        /// <summary>This method returns a list of similar elements in an Xml document.
        /// </summary>
        /// <param name="doc">The xml document.</param>
        /// <param name="rules">A bunch of rules for comparing. The rules are methods returning true if they elements are similar.</param>
        /// <returns></returns>
        public static IEnumerable<Similar> FindSimilars(
            XDocument doc,
            IEnumerable<Rule> rules
        )
        {
            if (doc == null) { throw new ArgumentNullException(nameof(doc)); }
            if (rules == null) { throw new ArgumentNullException(nameof(rules)); }

            var elements = Flatten(doc.Root).Where(e => e != doc.Root).ToList();
            for (var outerIndex = 0; outerIndex < elements.Count(); ++outerIndex)
            {
                var firstElement = elements[outerIndex];
                for (var innerIndex = outerIndex + 1; innerIndex < elements.Count(); ++innerIndex)
                {
                    var secondElement = elements[innerIndex];
                    if (firstElement != secondElement)
                    {
                        foreach (var rule in rules)
                        {
                            if (rule.Comparers.All(c => c(FirstElement: firstElement, SecondElement: secondElement)))
                            {
                                yield return Similar.Create(rule.RuleName, firstElement.ShallowCopy(), GetXPathOf(firstElement), secondElement.ShallowCopy(), GetXPathOf(secondElement));
                            }
                        }
                    }
                }
            }
        }

        #region Private helper methods.

        private static IEnumerable<Difference> Compare(XElement firstElement, XElement secondElement, int rowNum, FoundOnlyIn foundOnlyIn)
        {
            var diffs = new List<Difference>();

            var firstXPath = GetXPathOf(firstElement);
            var existingElementsInSecond = secondElement.Document.XPathSelectElements(firstXPath);
            if (existingElementsInSecond.Any() == false)
            {
                diffs.Add(Difference.Create(firstElement.ShallowCopy(), firstXPath, foundOnlyIn));
            }
            else if (existingElementsInSecond.Count() == 1)
            {
                // OK.
            }
            foreach (var child in firstElement.Elements())
            {
                diffs.AddRange(Compare(child, secondElement, 0, foundOnlyIn));
            }
            return diffs;
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

        /// <summary>This method returns the element and everyting below as a list of elements.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static IEnumerable<XElement> Flatten(XElement element)
        {
            // We can rewrite this method to yield. There is another recursive method that yields in this class.
            var ret = new List<XElement> { element };
            foreach (var e in element.Elements())
            {
                ret.AddRange(Flatten(e));
            }
            return ret;
        }

        /// <summary>This method returns the xpath with element names and attributes for an element.
        /// But! the root's part of the xpath is without attributes.
        /// <code>
        /// &lt;root path='c:\'&gt;
        ///     &lt;folder path = 'c:\documents\'/&gt;
        /// &lt;/ root &gt;
        /// returns
        /// "root/folder[@path = 'c:\documents\']"
        /// </code>
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static string GetXPathOf(XElement element)
        {
            var root = element.Parents().Last();
            var elementPath =
                "/" +
                element
                    .Parents()
                    .Reverse()
                    .Select(e => e.Name.LocalName + (e == root ? string.Empty : GetXPathOf(e.Attributes())))
                    .StringJoin("/");
            return elementPath;
        }

        /// <summary>This method returns the attributes as "an xpath string"
        /// Attributes like a='b' c='d' returns "[@a='b' and @c='d']".
        /// Note that the variable names anv valies are not escaped properlty.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
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
                ) +
                "]";

        }

        /// <summary>This methods returns all unique elements in an xml as a list.
        /// The list returns is a keyvalue list where
        /// the key is the element as string. e.g. &lt;Customer Name='Sisyfos'/&gt;
        /// the value is a list of xpathes to find the key.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static SortedList<string, IEnumerable<XElement>> ListOfElementsWithXpaths(XElement element)
        {
            var ret = new SortedList<string, IEnumerable<XElement>>();
            foreach (var e in Flatten(element))
            {
                var key = e.ShallowCopy().ToString();
                ret[key] =
                    ret.ContainsKey(key) ?
                        ret[key].Concat(new[] { e }) :
                        new[] { e };
            }
            return ret;
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

        #endregion

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
