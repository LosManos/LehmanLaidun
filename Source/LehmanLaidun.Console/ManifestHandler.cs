using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LehmanLaidun.Console
{
    internal class ManifestHandler
    {
        private readonly System.IO.Abstractions.FileSystem fileSystem;

        public ManifestHandler(System.IO.Abstractions.FileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }
        
        internal Manifest Read(string pathFile)
        {
            using (var fileStream = fileSystem.File.Open(pathFile, System.IO.FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(Manifest));
                var doc = (Manifest)serializer.Deserialize(fileStream)!;
                return doc;
            }
        }
    }

    public class Manifest
    {
        public string ManifestVersion { get; set; } = "1";

        [XmlArray("Dependencies")]
        [XmlArrayItem("Dependency")]
        public  PluginDependency[] Dependencies { get; set; } = Array.Empty<PluginDependency>();
    }

    [XmlRoot("Dependency")]
    public class PluginDependency
    {
        public string PathFile { get; set; } = "";
    }
}
