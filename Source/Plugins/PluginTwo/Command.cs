using LehmanLaidun.Plugin;
using System.Xml.Linq;

namespace PluginTwo
{
    public class Command : ICommand
    {
        public string Name => "Plugin two";

        public string Description => "The second plugin";

        public ParseResult Parse(string pathfile)
        {
            return ParseResult.Create(
                Name,
                XDocument.Parse($"<data plugin=\"{Name}\">{pathfile}</data>")
            );
        }
    }
}
