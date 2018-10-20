using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LehmanLaidun.FileSystem
{
    public class Duplicate
    {
        public XElement Element { get; }
        public IEnumerable<string> XPaths { get; }

        public static Duplicate Create(
            XElement element, 
            params string[] xPaths)
        {
            return new Duplicate(element, xPaths);
        }

        public static Duplicate Create(
            XElement element, 
            IEnumerable<string> xPaths)
        {
            if(element.HasElements) { throw new ArgumentException("The element must not have any children. Use ShallowCopy.", nameof(element)); }
            return new Duplicate(element, xPaths);
        }

        public override bool Equals(object obj)
        {
            var duplicate = obj as Duplicate;
            return duplicate != null &&
                   EqualityComparer<string>.Default.Equals(Element?.ToString(), duplicate.Element?.ToString()) &&
                   XPaths.SequenceEqual(duplicate.XPaths);
        }

        public override int GetHashCode()
        {
            var hashCode = -2117100120;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Element?.ToString());
            foreach( var xpath in XPaths)
            {
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(xpath);
            }
            return hashCode;
        }

        public static bool operator ==(Duplicate duplicate1, Duplicate duplicate2)
        {
            return EqualityComparer<Duplicate>.Default.Equals(duplicate1, duplicate2);
        }

        public static bool operator !=(Duplicate duplicate1, Duplicate duplicate2)
        {
            return !(duplicate1 == duplicate2);
        }

        private Duplicate(XElement element, IEnumerable<string> xPaths)
        {
            Element = element;
            XPaths = xPaths;
        }
    }
}
