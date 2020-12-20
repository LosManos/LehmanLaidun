using LehmanLaidun.Plugin;
using System.Drawing;
using System.Xml.Linq;

namespace ImagePlugin
{
    public class Command : ICommand
    {
        public string Name => "Image plugin";

        public string Description => "This plugin returns image information.";

        public ParseResult Parse(string pathfile)
        {
            var image = Image.FromFile(pathfile);

            var xml = new XDocument(
                new XElement("image",
                    new XAttribute("width", image.Width),
                    new XAttribute("height", image.Height)
                )
            );

            //  There is more info for an image.
            //  One is Flags https://docs.microsoft.com/en-us/dotnet/api/system.drawing.image.flags
            //  Another is properties.
            //  https://docs.microsoft.com/en-us/dotnet/desktop/winforms/advanced/how-to-read-image-metadata
            //  https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.propertyitem.id

            return ParseResult.Create(
                Name,
                xml);
        }
    }
}
