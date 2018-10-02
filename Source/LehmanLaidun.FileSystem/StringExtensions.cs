using System.Collections.Generic;

namespace LehmanLaidun.FileSystem
{
    public static class StringExtensions
    {
        public static string StringJoin(this IEnumerable<string> lst, string separator)
        {
            return string.Join(separator, lst);
        }
    }
}
