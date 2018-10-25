using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace LehmanLaidun.FileSystem
{
    public class Similar
    {
        public XElement Element { get; }
        public string Xpath { get; }

        private Similar(
            XElement element,
            string xPath)
        {
            if (element.HasElements) { throw new ArgumentException("The element must not have any children. Use ShallowCopy.", nameof(element)); }
            Element = element;
            Xpath = xPath;
        }

        public static Similar Create(XElement element, string xpath)
        {
            return new Similar(element, xpath);
        }

        public override bool Equals(object obj)
        {
            var similar = obj as Similar;
            return similar != null &&
                   EqualityComparer<string>.Default.Equals(Element?.ToString(), similar.Element?.ToString()) &&
                   Xpath == similar.Xpath;
        }

        public override int GetHashCode()
        {
            var hashCode = 801317247;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Element?.ToString());
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Xpath);
            return hashCode;
        }

        public static bool operator ==(Similar similar1, Similar similar2)
        {
            return EqualityComparer<Similar>.Default.Equals(similar1, similar2);
        }

        public static bool operator !=(Similar similar1, Similar similar2)
        {
            return !(similar1 == similar2);
        }
    }
}
