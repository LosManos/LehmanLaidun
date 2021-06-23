using C = System.Console;

namespace LehmanLaidun.Console
{
    internal class Outputter : IOutputter
    {
        internal static Outputter Create()
        {
            return new Outputter();
        }
        void IOutputter.WriteLine(string message)
        {
            C.WriteLine(message);
        }
    }
}