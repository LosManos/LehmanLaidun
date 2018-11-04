using System.Collections.Generic;
using System.Xml.Linq;
using static LehmanLaidun.FileSystem.Difference;

namespace LehmanLaidun.FileSystem.Test
{
    partial class ControlTest
    {
        private IEnumerable<CanCompareXmlTestDataClass> CanCompareXmlTestData
        {
            get
            {
                // Indata with stuff found only in the *first* tree.
                //
                yield return new CanCompareXmlTestDataClass(
                    "The element should be found only in the first tree.",
                    "<root><a/></root>",
                    "<root></root>",
                    Difference.Create(new XElement("a"), "/root/a[not(@*)]", FoundOnlyIn.First)
                );
                yield return new CanCompareXmlTestDataClass(
                    "The inner element should be found only in the first tree.",
                    "<root><a><b/></a></root>",
                    "<root><a/></root>",
                    Difference.Create(new XElement("b"), "/root/a[not(@*)]/b[not(@*)]", FoundOnlyIn.First)
                );
                yield return new CanCompareXmlTestDataClass(
                    "Element with attributes differs from one without. The attributes are in the first tree.",
                    "<root><a b='c'/></root>",
                    "<root><a/></root>",
                    Difference.Create(new XElement("a", new XAttribute("b", "c")), "/root/a[@b='c']", FoundOnlyIn.First),
                    Difference.Create(new XElement("a"), "/root/a[not(@*)]", FoundOnlyIn.Second)
                );
                yield return new CanCompareXmlTestDataClass(
                    "The sequence elements should be found only in the first tree.",
                    "<root><a/><b/></root>",
                    "<root></root>",
                        Difference.Create(new XElement("a"), "/root/a[not(@*)]", FoundOnlyIn.First),
                        Difference.Create(new XElement("b"), "/root/b[not(@*)]", FoundOnlyIn.First)
                );

                //  Testdata with stuff found only in the *second* tree.
                //
                yield return new CanCompareXmlTestDataClass(
                    "The element should be found only in the second tree.",
                    "<root></root>",
                    "<root><a/></root>",
                    Difference.Create(new XElement("a"), "/root/a[not(@*)]", FoundOnlyIn.Second)
                );
                yield return new CanCompareXmlTestDataClass(
                    "The inner element should be found only in the second tree.",
                    "<root><a/></root>",
                    "<root><a><b/></a></root>",
                    Difference.Create(new XElement("b"), "/root/a[not(@*)]/b[not(@*)]", FoundOnlyIn.Second)
                    );
                yield return new CanCompareXmlTestDataClass(
                    "The element with attributes should be found only in the second tree.",
                    "<root><a/></root>",
                    "<root><a b='c'/></root>",
                    Difference.Create(new XElement("a"), "/root/a[not(@*)]", FoundOnlyIn.First),
                    Difference.Create(new XElement("a", new XAttribute("b", "c")), "/root/a[@b='c']", FoundOnlyIn.Second)
                );
                yield return new CanCompareXmlTestDataClass(
                    "The elements should be found only in the second tree.",
                    "<root></root>",
                    "<root><a/><b/></root>",
                    Difference.Create(new XElement("a"), "/root/a[not(@*)]", FoundOnlyIn.Second),
                    Difference.Create(new XElement("b"), "/root/b[not(@*)]", FoundOnlyIn.Second)
                );
                yield return new CanCompareXmlTestDataClass(
                    "Elements with only attributes differing should be found.",
                    "<root><a b='c'/></root>",
                    "<root><a d='e'/></root>",
                    Difference.Create(new XElement("a", new XAttribute("b", "c")), "/root/a[@b='c']", FoundOnlyIn.First),
                    Difference.Create(new XElement("a", new XAttribute("d", "e")), "/root/a[@d='e']", FoundOnlyIn.Second)
                );
            }
        }

        private static IEnumerable<object[]> DuplicateTestData
        {
            get
            {
                yield return DuplicateTestDataClass.Create(
                    @"<root/>",
                    new DuplicateTestDataClass.ElementAndXPaths[0],
                    "No duplicate in simple root.")
                    .ToObjectArray();

                yield return DuplicateTestDataClass.Create(
                    @"
                    <root>
                        <d n='a'>
                            <f n='a'/>
                            <f n='b'/>
                        </d>
                        <d n='b'>
                            <f n='a'/>
                            <f n='c'/>
                        </d>
                    </root>",
                    new[]
                    {
                        DuplicateTestDataClass.ElementAndXPaths.Create(
                            @"<f n='a'/>",
                            new[]{
                                @"/root/d[@n='a']/f[@n='a']",
                                @"/root/d[@n='b']/f[@n='a']"
                            }
                        )
                    },
                    "Duplicate files found in sibling directories.")
                    .ToObjectArray();

                yield return DuplicateTestDataClass.Create(
                    @"
                    <root>
                        <d n='a'>
                            <f n='a'/>
                            <f n='c'/>
                            <d n='b'>
                                <f n='b'/>
                                <f n='a'/>
                            </d>
                        </d>
                    </root>",
                    new[]
                    {
                        DuplicateTestDataClass.ElementAndXPaths.Create
                        (
                            @"<f n='a'/>",
                            new[]{
                                @"/root/d[@n='a']/f[@n='a']",
                                @"/root/d[@n='a']/d[@n='b']/f[@n='a']"
                            }
                        )
                    },
                    "Duplicate files found in parent/child directories.")
                    .ToObjectArray();
                }
        }

        private static IEnumerable<object[]> SimilarTestData
        {
            get
            {
                yield return SimilarTestDataClass.Create(
                    @"<root/>",
                    new Logic.Rule[] { },
                    new (string, string)[] { },
                    "No similaries found in an empty input."
                ).ToObjectArray();

                yield return SimilarTestDataClass.Create(
                    @"
                    <root>
                        <d Name='a'>
                            <f Name='b' Size='12'/>
                            <f Name='d' Size='123'/>
                        </d>
                        <d Name='c'>
                            <f Name='b' Size='13'/>
                        </d>
                    </root>",
                    new[] {
                        Logic.Rule.Create(
                        "Just a single arbitrary rule - Equal Name but different Size",
                        new Logic.Rule.ComparerDelegate [] {
                            (firstElement, secondElement) => {
                                return
                                    firstElement.Name != "d" &&
                                    secondElement.Name != "d" &&
                                    firstElement.Attribute("Name").Value ==
                                    secondElement.Attribute("Name").Value &&
                                    firstElement.Attribute("Size").Value !=
                                    secondElement.Attribute("Size").Value;
                            }
                        }
                    )   },
                    new (string, string)[]{
                        (@"/root/d[@Name='a']/f[@Name='b' and @Size='12']",
                        @"/root/d[@Name='c']/f[@Name='b' and @Size='13']")
                    },
                    "A single rule should be executed."
                ).ToObjectArray();

                yield return SimilarTestDataClass.Create(
                    @"
                    <root>
                        <d n='a'>
                            <f n='b' s='12'/>
                            <f n='c' s='123'/>
                        </d>
                        <d n='c'>
                            <f n='c' s='13'/>
                        </d>
                    </root>",
                  new[] {
                        Logic.Rule.Create(
                            "An arbitrary rule - any element with a n=b tag.",
                            new Logic.Rule.ComparerDelegate [] {
                                (firstElement, secondElement ) => {
                                    return
                                        firstElement.Name == "f" &&
                                        secondElement.Name == "f" &&
                                        firstElement.Attribute("n")?.Value == "b" &&
                                        secondElement.Attribute("n")?.Value == "b";
                                }
                            }
                        ),
                        Logic.Rule.Create(
                            "An arbitrary rule - any element with a c tag.",
                            new Logic.Rule.ComparerDelegate [] {
                                (firstElement, secondElement) => {
                                    return
                                        firstElement.Name == "f" &&
                                        secondElement.Name == "f" &&
                                        firstElement.Attribute("n")?.Value == "c" &&
                                        secondElement.Attribute("n")?.Value == "c";
                                }
                            }
                        )
                  },
                    new (string, string)[]{
                        (@"/root/d[@n='a']/f[@n='c' and @s='123']",
                        @"/root/d[@n='c']/f[@n='c' and @s='13']")
                    },
                    "Several rules should be executed."
                    ).ToObjectArray();
            }
        }

        private class CanCompareXmlTestDataClass
        {
            internal string FirstXml { get; }
            internal string SecondXml { get; }
            internal IEnumerable<Difference> Differences { get; }
            internal string Message { get; }

            internal CanCompareXmlTestDataClass(
                string message,
                string firstXml,
                string secondXml,
                params Difference[] differences)
            {
                Message = message;
                FirstXml = firstXml;
                SecondXml = secondXml;
                Differences = differences;
            }
        }

        public class DuplicateTestDataClass
        {
            public class ElementAndXPaths
            {
                internal string ElementString { get; }
                internal IEnumerable<string> Xpaths { get; }
                internal static ElementAndXPaths Create(string elementString, IEnumerable<string> xpaths)
                {
                    return new ElementAndXPaths(elementString, xpaths);
                }
                private ElementAndXPaths(string elementString, IEnumerable<string> xpaths)
                {
                    ElementString = elementString;
                    Xpaths = xpaths;
                }
            }

            internal ElementAndXPaths[] ElementStringAndXPaths { get; }
            internal string Message { get; }
            internal string Xml { get; }

            internal static DuplicateTestDataClass Create(string xml, ElementAndXPaths[] elementStringAndXPaths, string message)
            {
                return new DuplicateTestDataClass(xml, elementStringAndXPaths, message);
            }

            internal object[] ToObjectArray()
            {
                return new object[]
                {
                    Xml,
                    ElementStringAndXPaths,
                    Message
                };
            }

            private DuplicateTestDataClass(string xml, ElementAndXPaths[] elementStringAndXPaths, string message)
            {
                Xml = xml;
                ElementStringAndXPaths = elementStringAndXPaths;
                Message = message;
            }
        }

        private class SimilarTestDataClass
        {
            internal XDocument Xml { get; private set; }
            internal Logic.Rule[] Rules { get; private set; }
            internal IEnumerable<(string, string)> ExpectedXPaths { get; private set; }
            internal string Message { get; private set; }

            internal static SimilarTestDataClass Create(
                string xmlString,
                Logic.Rule[] rules,
                IEnumerable<(string, string)> expectedXPaths,
                string message
                )
            {
                return new SimilarTestDataClass(
                    xmlString,
                    rules,
                    expectedXPaths,
                    message);
            }

            private SimilarTestDataClass(
                    string xmlString,
                    Logic.Rule[] rules,
                    IEnumerable<(string, string)> expectedXpaths,
                    string message)
            {
                Xml = XDocument.Parse(xmlString);
                ExpectedXPaths = expectedXpaths;
                Rules = rules;
                Message = message;
            }

            internal object[] ToObjectArray()
            {
                return new object[]
                {
                    Xml,
                    Rules,
                    ExpectedXPaths,
                    Message
                };
            }
        }

    }
}
