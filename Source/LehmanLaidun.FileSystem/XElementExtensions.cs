using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace LehmanLaidun.FileSystem
{
    internal static class XElementExtensions
    {
        private const string ElementNameDirectory = "directory";
        private const string ElementNameFile = "file";
        private const string ElementNameRoot = "root";
        private const string AttributeNameLength = "length";
        private const string AttributeNameName = "name";
        private const string AttributeNamePath = "path";

        /// <summary>This method is the same as Add but it also returns the argument.
        /// Makes for neater calling code.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        internal static XElement AddElement(this XElement me, XElement element)
        {
            me.Add(element);
            return element;
        }

        internal static XElement AddDirectoryElement( this XElement element, string directory)
        {
            var newElement = new XElement(ElementNameDirectory, new XAttribute(AttributeNamePath, directory));
            element.AddElement(newElement);
            return newElement;
        }

        /// <summary>This is an ordinary Add but it also returns itself.
        /// Handy for chaining calls.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        internal static XElement AddElements(this XElement element, IEnumerable<XElement> elements)
        {
            element.Add(elements);
            return element;
        }

        /// <summary>This method creates an element out of the FileItem provided and adds it to the element provided.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        internal static XElement AddFileElement(this XElement element, FileItem file)
        {
            var newElement = new XElement(
                ElementNameFile,
                new XAttribute(AttributeNameName, file.Name),
                new XAttribute(AttributeNameLength, file.Length)
            );
            element.AddElement(newElement);
            return newElement;
        }

        /// <summary>This method returns a list of all parents to the supplied element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        internal static IEnumerable<XElement> Parents(this XElement element)
        {
            var ret = new List<XElement> { element };
            var e = element;
            while( e.Parent != null)
            {
                e = e.Parent;
                ret.Add(e);
            }
            return ret;
        }

        /// <summary>This method returns the directory element found from a list of directory names, uppermost directory first..
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="directoryNames"></param>
        /// <returns></returns>
        internal static XElement SelectDirectoryElement(this XDocument doc, IEnumerable<string> directoryNames)
        {
            return doc.XPathSelectElement(CreateXPathFrom(directoryNames));
        }

        /// <summary>This method does a shallow copy, only the element name and the attributes
        /// of an element.
        /// The normal copy constructor does a deept copy.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        internal static XElement ShallowCopy(this XElement element)
        {
            var ret = new XElement(element.Name);
            ret.Add(element.Attributes());
            return ret;
        }

        #region Private helper methods.

        private static string CreateXPathFrom(IEnumerable<string> directories)
        {
            return "/" +
                    string.Join(
                        "/",
                        Join(
                            ElementNameRoot,
                            directories.Select(d => $"{ElementNameDirectory}[@{AttributeNamePath}= '{d}']")
                    ));
        }

        private static IEnumerable<string> Join(string item, IEnumerable<string> items)
        {
            var x = items.ToList();
            x.Insert(0, item);
            return x;
        }

        #endregion
    }
}
