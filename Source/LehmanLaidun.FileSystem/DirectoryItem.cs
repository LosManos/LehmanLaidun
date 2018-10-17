using System.Collections.Generic;

namespace LehmanLaidun.FileSystem
{
    public class DirectoryItem : FileSystemItem
    {
        public string Name { get; }

        private DirectoryItem(string name)
        {
            Name = name;
        }
        
        public static DirectoryItem Create(string name)
        {
            return new DirectoryItem(name);
        }

        public override bool Equals(object obj)
        {
            var item = obj as DirectoryItem;
            return item != null &&
                   Name == item.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }

        public static bool operator ==(DirectoryItem item1, DirectoryItem item2)
        {
            return EqualityComparer<DirectoryItem>.Default.Equals(item1, item2);
        }

        public static bool operator !=(DirectoryItem item1, DirectoryItem item2)
        {
            return !(item1 == item2);
        }
    }
}