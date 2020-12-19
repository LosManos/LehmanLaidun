using LehmanLaidun.Plugin;
using System.Xml.Linq;

namespace ImagePlugin
{
    public class Command : ICommand
    {
        public string Name => "Image plugin";

        public string Description => "This plugin returns image information.";

        public ParseResult Parse(string pathfile)
        {
            var xml = new XDocument(
                new XElement("image")
            );
            return ParseResult.Create(
                Name,
                xml);
        }
    }
}
