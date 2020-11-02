using LehmanLaidun.Plugin;
using System.Xml.Linq;

namespace PluginOne
{
    public class Command : ICommand
    {
        public string Name => "Plugin one";

        public string Description => "The first plugin";

        public ParseResult Parse(string pathfile)
        {
            return ParseResult.Create(
                Name,
                XDocument.Parse($"<data plugin=\"{Name}\">{pathfile}</data>")
            );
        }
    }
}
    