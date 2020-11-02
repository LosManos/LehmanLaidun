using System.Collections.Generic;
using System.Xml.Linq;

namespace LehmanLaidun.Plugin
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }

        ParseResult Parse(string pathfile);
    }

    public class ParseResult
    {
        public string Name { get; }
        public XDocument Result { get; }
        private ParseResult(string name, XDocument result)
        {
            Name = name;
            Result = result;
        }
        public static ParseResult Create(string name, XDocument result)
        {
            return new ParseResult(name, result);
        }

        public override bool Equals(object? obj)
        {
            return obj is ParseResult result &&
                   Name == result.Name &&
                   EqualityComparer<XDocument>.Default.Equals(Result, result.Result);
        }

        public override int GetHashCode()
        {
            int hashCode = 1886149178;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<XDocument>.Default.GetHashCode(Result);
            return hashCode;
        }

        public static bool operator ==(ParseResult? left, ParseResult? right)
        {
            if (left == null && right == null) { return true; }
            else if (left == null || right == null) { return false; }
            return EqualityComparer<ParseResult>.Default.Equals(left, right);
        }

        public static bool operator !=(ParseResult? left, ParseResult? right)
        {
            return !(left == right);
        }
    }
}
