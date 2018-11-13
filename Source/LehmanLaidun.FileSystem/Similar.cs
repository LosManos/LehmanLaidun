using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace LehmanLaidun.FileSystem
{
    public class Similar
    {
        public string RuleName { get; }
        public XElement FirstElement { get; }
        public string FirstXpath { get; }
        public XElement SecondElement { get; }
        public string SecondXpath { get; }

        private Similar(
            string ruleName,
            XElement firstElement,
            string firstXpath, 
            XElement secondElement,
            string secondXpath)
        {
            RuleName = ruleName;
            FirstElement = firstElement;
            SecondElement = secondElement;
            FirstXpath = firstXpath;
            SecondXpath = secondXpath;
        }

        public static Similar Create( string ruleName, string firstXpath, string secondXpath)
        {
            return Create(
                ruleName,
                LastElementOf(firstXpath),
                firstXpath,
                LastElementOf(secondXpath),
                secondXpath);
        }

        public static Similar Create(string ruleName, XElement firstElement, string firstXpath, XElement secondElement, string secondXpath)
        {
            Func<XElement, string, bool> elementEqualsLastElementInXpath = (element, xpath) =>
       element.ToString() == LastElementOf(xpath).ToString();

            if (ruleName == null) { throw new ArgumentNullException(nameof(ruleName)); }
            if (firstElement.HasElements) { throw new ArgumentException("The first element must not have any children. Use ShallowCopy.", nameof(firstElement)); }
            if (secondElement.HasElements) { throw new ArgumentException("The second element must not have any children. Use ShallowCopy.", nameof(secondElement)); }
            if (elementEqualsLastElementInXpath(firstElement, firstXpath) == false) { throw new FirstElementAndXpathDoNotMatchException(ruleName, firstElement.ToString(), firstXpath); }
            if (elementEqualsLastElementInXpath(secondElement, secondXpath) == false) { throw new SecondElementAndXpathDoNotMatchException(ruleName, secondElement.ToString(), secondXpath); }

            return new Similar(ruleName, firstElement, firstXpath, secondElement, secondXpath);
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

        #region Private methods.

        private static XElement LastElementOf(string xpath)
        {
            var lastElementString = xpath.Split('/').Last();
            var matches = Regex.Match(lastElementString, @"(.*)\[(.*)\]");
            var name = matches.Groups[1].Value;
            var attributes = matches.Groups[2].Value.Split(new[] { "and" }, StringSplitOptions.None)
               .Select(x =>
               {
                   var nameValuePair = x.Split(new[] { "=" }, StringSplitOptions.None);
                   return (
                       name: nameValuePair.First().Trim().TrimStart('@').Trim(),
                       value: nameValuePair.Last().Trim().TrimStart('\'').TrimEnd('\'')
                   );
               });
            return new XElement(
               name,
               attributes.Select(a => new XAttribute(a.name, a.value)));
        }

        #endregion
    }
}
