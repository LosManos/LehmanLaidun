using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace LehmanLaidun.FileSystem
{
    public class Similar
    {
        public XElement FirstElement { get; }
        public XElement SecondElement { get; }
        public string FirstXpath { get; }
        public string SecondXpath { get; }

        private Similar(
            XElement firstElement,
            string firstXpath, 
            XElement secondElement,
            string secondXpath)
        {
            if (firstElement.HasElements) { throw new ArgumentException("The first element must not have any children. Use ShallowCopy.", nameof(firstElement)); }
            if (secondElement.HasElements) { throw new ArgumentException("The second element must not have any children. Use ShallowCopy.", nameof(secondElement)); }
            FirstElement = firstElement;
            SecondElement = secondElement;
            FirstXpath = firstXpath;
            SecondXpath = secondXpath;
        }

        public static Similar Create(XElement firstElement, string firstXpath, XElement secondElement, string secondXpath)
        {
            return new Similar(firstElement, firstXpath, secondElement, secondXpath);
        }

        public override bool Equals(object obj)
        {
            var similar = obj as Similar;
            return similar != null &&
                   EqualityComparer<string>.Default.Equals(FirstElement?.ToString(), similar.FirstElement?.ToString()) &&
                   FirstXpath == similar.FirstXpath &&
                   EqualityComparer<string>.Default.Equals(SecondElement?.ToString(), similar.SecondElement?.ToString()) &&
                   SecondXpath == similar.SecondXpath;
        }

        public override int GetHashCode()
        {
            var hashCode = 801317247;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FirstElement?.ToString());
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FirstXpath);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SecondElement?.ToString());
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SecondXpath);
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
