namespace LehmanLaidun.Console.Test
{
    internal static class StringExtensions
    {
        internal static string NormaliseLineEndings( this string s)
        {
            return s.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
        }
    }
}
