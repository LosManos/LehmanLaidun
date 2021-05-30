using LehmanLaidun.Plugin;
using System.Xml.Linq;

namespace ExifPlugin
{
    public class Command : ICommand
    {
        public string Name => "Exif plugin";

        public string Description => "This plugin returns Exif information.";

        public ParseResult Parse(string pathfile)
        {
            string model = null;
            using (var reader = new ExifLib.ExifReader(pathfile))
            {
                reader.GetTagValue<string>(ExifLib.ExifTags.Model, out model);
            }

            var xml = new XDocument(
                new XElement("exif",
                    new XAttribute("model", model ?? "null")
                )
            );

            return ParseResult.Create(
                Name, 
                xml );
        }
    }
}
