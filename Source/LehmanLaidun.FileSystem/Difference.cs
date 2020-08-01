using System.Xml.Linq;
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LehmanLaidun.FileSystem.Test")]

namespace LehmanLaidun.FileSystem
{
    public class Difference
    {
        public enum FoundOnlyIn
        {
            First,
            Second
        }

        public XElement? FirstElement { get; private set; }
        public string? FirstXPath { get; }
        public XElement? SecondElement { get; private set; }
        public string? SecondXPath { get; }

        internal static Difference Create(XElement element, string xpath, FoundOnlyIn foundOnlyIn)
        {
            if( foundOnlyIn == FoundOnlyIn.First)
            {
                return new Difference(element, xpath, null, null); 
            }
            else
            {
                return new Difference(null, null, element, xpath);
            }
        }

        private Difference(XElement? firstElement, string? firstXPath, XElement? secondElement, string? secondXPath)
        {
            FirstElement = firstElement;
            FirstXPath = firstXPath;
            SecondElement = secondElement;
            SecondXPath = secondXPath;
        }
    }
}