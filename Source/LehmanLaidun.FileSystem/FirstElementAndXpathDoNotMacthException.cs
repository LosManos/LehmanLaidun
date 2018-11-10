using System;

namespace LehmanLaidun.FileSystem
{
    [Serializable]
    public class ElementAndXpathDoNotMatchException : Exception
    {
        public string Element { get; }
        public string Xpath { get; }

        public ElementAndXpathDoNotMatchException(
            string element, 
            string xpath)
            :base($"Element {element} does not match last element in xpath {xpath}")
        {
            Element = element;
            Xpath = xpath;
        }
        public ElementAndXpathDoNotMatchException() { }
        public ElementAndXpathDoNotMatchException(string message) : base(message) { }
        public ElementAndXpathDoNotMatchException(string message, Exception inner) : base(message, inner) { }
        protected ElementAndXpathDoNotMatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class FirstElementAndXpathDoNotMatchException : ElementAndXpathDoNotMatchException
    {
        public FirstElementAndXpathDoNotMatchException(
            string element,
            string xpath
            ) : base(element, xpath)
        { }
        public FirstElementAndXpathDoNotMatchException() { }
        public FirstElementAndXpathDoNotMatchException(string message) : base(message) { }
        public FirstElementAndXpathDoNotMatchException(string message, Exception inner) : base(message, inner) { }
        protected FirstElementAndXpathDoNotMatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class SecondElementAndXpathDoNotMatchException : ElementAndXpathDoNotMatchException
    {
        public SecondElementAndXpathDoNotMatchException(
            string element,
            string xpath
            ) : base(element, xpath)
        { }
        public SecondElementAndXpathDoNotMatchException() { }
        public SecondElementAndXpathDoNotMatchException(string message) : base(message) { }
        public SecondElementAndXpathDoNotMatchException(string message, Exception inner) : base(message, inner) { }
        protected SecondElementAndXpathDoNotMatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
