using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace LehmanLaidun.FileSystem.Test
{
    [TestClass]
    public partial class PluginHandlerTest
    {
        [TestMethod]
        public void ShouldLoadAndExecute()
        {
            var sourceCode = @"
using LehmanLaidun.Plugin;
using System.Xml.Linq;

namespace MyPlugin
{
    public class Command : ICommand
    {
        public string Name => ""My plugin"";

        public string Description => ""A plugin for testing"";

        public ParseResult Parse(string pathfile)
        {
            return ParseResult.Create(
                Name,
                XDocument.Parse($""<data plugin=\""{Name}\"">{pathfile}</data>"")
            );
        }
    }
}
";
            var assembly = CreateAssembly(sourceCode);
            var sut = PluginHandler.Create();

            //  Act.
            sut.Load(new[] { assembly });
            var res = sut.Execute("x");

            //  Assert.
            res
                .Select(pr => new { pr.Name, Result = pr.Result.ToString() })
                .Should()
                .BeEquivalentTo(
                    new[]{
                        new { Name = "My plugin", Result = "<data plugin=\"My plugin\">x</data>" },
                    }
                );
        }

        private static byte[] Compile(string sourceCode)
        {
            using (var peStream = new MemoryStream())
            {
                var result = GenerateCode(sourceCode).Emit(peStream);
                if (result.Success == false)
                {
                    throw new System.Exception(string.Join(System.Environment.NewLine, result.Diagnostics.Select(d => d.ToString())));
                }
                peStream.Seek(0, SeekOrigin.Begin);
                return peStream.ToArray();
            }
        }

        private static Assembly CreateAssembly(string sourceCode)
        {
            var compiledAssembly = Compile(sourceCode);
            using (var asm = new MemoryStream(compiledAssembly))
            {
                var assemblyLoadContext = new AssemblyLoadContext("Plugin", true);
                var assembly = assemblyLoadContext.LoadFromStream(asm);
                return assembly;
            }
        }

        private static CSharpCompilation GenerateCode(string sourceCode)
        {
            var codeString = SourceText.From(sourceCode);
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp8);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

            var references = new[]
            {
                // The commented code below is presumably needed for whatever to get dynamic loading to work.
                // But it seems not to be necessary.
                //MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                //MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),

                // Needed for Plugin implementation.
                // The netstandard version is hard coded here but should be taken from teh LehmanLaidun.Plugin project dynamically.
                MetadataReference.CreateFromFile( Path.Combine(
                    UserProfilePath, ".nuget", "packages", "netstandard.library", "2.0.3", "build", "netstandard2.0", "ref", "netstandard.dll")),
                MetadataReference.CreateFromFile(typeof(Plugin.ICommand).Assembly.Location),
            };

            return CSharpCompilation.Create("Plugin.dll",
                new[] { parsedSyntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Debug,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }

        private static string UserProfilePath => System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
    }
}
