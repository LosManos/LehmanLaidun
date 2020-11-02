namespace LehmanLaidun.Console.Integration.Test
{
    internal static class StringExtensions
    {
        internal static string NormaliseLineEndings( this string s)
        {
            return s.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
        }
    }
}
