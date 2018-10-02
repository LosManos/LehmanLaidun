using System;
using System.Xml.Linq;

namespace LehmanLaidun.FileSystem
{
    public class Difference
    {
        public XElement FirstElement { get; private set; }
        public string FirstXPath { get; }
        public XElement SecondElement { get; private set; }
        public string SecondXPath { get; }

        // TODO:Make internal.
        public static Difference Create(XElement firstElement, string firstXPath, XElement secondElement, string secondXPath)
        {
            return new Difference(firstElement, firstXPath, secondElement, secondXPath);
        }

        internal static Difference Create(XElement element1, XElement element2, int count)
        {
            throw new NotImplementedException();
        }

        private Difference(XElement firstElement, string firstXPath, XElement secondElement, string secondXPath)
        {
            FirstElement = firstElement;
            FirstXPath = firstXPath;
            SecondElement = secondElement;
            SecondXPath = secondXPath;
        }
    }
}