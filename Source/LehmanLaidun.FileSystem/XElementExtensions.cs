using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace LehmanLaidun.FileSystem
{
    public static class XElementExtensions
    {
        /// <summary>This method is the same as Add but it also returns the argument.
        /// Makes for neater calling code.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static XElement AddElement(this XElement me, XElement element)
        {
            me.Add(element);
            return element;
        }

        //public static XElement FindLongestPath(this XDocument doc, IEnumerable<string> directories)
        //{
        //    for (int i = directories.Count() - 1; i >= 0; --i)
        //    {
        //        var xpath = XPath(directories.Take(i));
        //        var res = doc.Document.XPathSelectElement(xpath);
        //        if (res != null) { return res; }
        //    }
        //    return null;
        //}

        public static XElement AddDirectoryElement( this XElement element, string directory)
        {
            var newElement = new XElement("directory", new XAttribute("path", directory));
            element.AddElement(newElement);
            return newElement;
        }

        public static XElement AddFileElement(this XElement element, FileItem file)
        {
            var newElement = new XElement("file", new XAttribute("name", file.Name));
            element.AddElement(newElement);
            return newElement;
        }

        public static XElement SelectDirectoryElement(this XDocument doc, IEnumerable<string> directoryNames)
        {
            return doc.XPathSelectElement(XPath(directoryNames));
        }

        private static IEnumerable<string> Join(string item, IEnumerable<string> items)
        {
            var x = items.ToList();
            x.Insert(0, item);
            return x;
        }

        private static string XPath(IEnumerable<string> directories)
        {
            return "/" +
                    string.Join(
                        "/",
                        Join(
                            "root",
                            directories.Select(d => $"directory[@path='{d}']")
                    ));
        }
    }
}
