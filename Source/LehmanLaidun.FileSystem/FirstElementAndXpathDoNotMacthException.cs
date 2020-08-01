using System;

namespace LehmanLaidun.FileSystem
{
    [Serializable]
    public class ElementAndXpathDoNotMatchException : Exception
    {
        public string RuleName { get; } = "";
        public string Element { get; } = "";
        public string Xpath { get; } = "";

        public ElementAndXpathDoNotMatchException(
            string ruleName,
            string element,
            string xpath)
            : base($"Element {element} does not match last element in xpath {xpath} for rule {ruleName}.") {
            RuleName = ruleName;
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
            string ruleName,
            string element,
            string xpath
            ) : base(ruleName, element, xpath)
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
            string ruleName,
            string element,
            string xpath
            ) : base(ruleName, element, xpath)
        { }
        public SecondElementAndXpathDoNotMatchException() { }
        public SecondElementAndXpathDoNotMatchException(string message) : base(message) { }
        public SecondElementAndXpathDoNotMatchException(string message, Exception inner) : base(message, inner) { }
        protected SecondElementAndXpathDoNotMatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
