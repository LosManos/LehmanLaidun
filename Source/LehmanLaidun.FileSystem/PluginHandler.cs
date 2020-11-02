using LehmanLaidun.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LehmanLaidun.FileSystem
{
    public interface IPluginHandler
    {
        IEnumerable<ParseResult> Execute(string pathfile);
        void Load(IEnumerable<Assembly> assemblies);
    }

    public class PluginHandler : IPluginHandler
    {
        private static IList<ICommand> commands = new List<ICommand>();

        private PluginHandler() { }

        public static PluginHandler Create()
        {
            return new PluginHandler();
        }

        public void Load(IEnumerable<Assembly> assemblies)
        {
            commands = assemblies.SelectMany(assembly => CreateCommands(assembly)).ToList();
        }

        public IEnumerable<ParseResult> Execute(string pathfile)
        {
            foreach (var command in commands)
            {
                yield return command.Parse(pathfile);
            }
        }

        private static IEnumerable<ICommand> CreateCommands(Assembly assembly)
        {
            var count = 0;

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(ICommand).IsAssignableFrom(type))
                {
                    if (Activator.CreateInstance(type) is ICommand result)
                    {
                        count++;
                        yield return result;
                    }
                }
            }

            if (count == 0)
            {
                string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                throw new ApplicationException(
                    $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }
    }
}
