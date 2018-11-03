using System;
using System.Collections.Generic;
using System.Linq;
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

        private static IEnumerable<object[]> SimilarTestData
        {
            get
            {
                yield return SimilarTestDataClass.Create(
                    @"<root/>",
                    new (
                        string,
                        Func<
                            (
                                XElement,
                                XElement
                            ),
                            bool>[]
                    )[] { },
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
                        (
                        "Just a single arbitrary rule - Equal Name but different Size",
                        new Func<
                            (
                                XElement FirstElement,
                                XElement SecondElement
                            ),
                            bool>[] {
                            comparer => {
                                return
                                    comparer.FirstElement.Name != "d" &&
                                    comparer.SecondElement.Name != "d" &&
                                    comparer.FirstElement.Attribute("Name").Value ==
                                    comparer.SecondElement.Attribute("Name").Value &&
                                    comparer.FirstElement.Attribute("Size").Value !=
                                    comparer.SecondElement.Attribute("Size").Value;
                            }
                        }
                    )   },
                    new (string,string)[]{
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
                        (
                            "An arbitrary rule - any element with a n=b tag.",
                            new Func<
                                (
                                    XElement FirstElement,
                                    XElement SecondElement
                                ),
                                bool>[] {
                                comparer => {
                                    return 
                                        comparer.FirstElement.Name == "f" &&
                                        comparer.SecondElement.Name == "f" &&
                                        comparer.FirstElement.Attribute("n")?.Value == "b" &&
                                        comparer.SecondElement.Attribute("n")?.Value == "b";
                                }
                            }
                        ), 
                        (
                            "An arbitrary rule - any element with a c tag.",
                            new Func<
                                (
                                    XElement FirstElement,
                                    XElement SecondElement
                                ),
                                bool>[] {
                                comparer => {
                                    return
                                        comparer.FirstElement.Name == "f" &&
                                        comparer.SecondElement.Name == "f" &&
                                        comparer.FirstElement.Attribute("n")?.Value == "c" &&
                                        comparer.SecondElement.Attribute("n")?.Value == "c";
                                }
                            }
                        )
                  },
                    new (string,string)[]{
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

            [Obsolete("Replace with the one taking [message] as first argument", false)]
            internal CanCompareXmlTestDataClass(string firstXml, string secondXml, Difference difference, string message)
                : this(firstXml, secondXml, new[] { difference }, message)
            {
            }

            [Obsolete("Replace with the one taking [message] as first argument", false)]
            private CanCompareXmlTestDataClass(string firstXml, string secondXml, IEnumerable<Difference> differences, string message)
            {
                FirstXml = firstXml;
                SecondXml = secondXml;
                Differences = differences;
                Message = message;
            }

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

        internal class SimilarTestDataClass
        {
            internal XDocument Xml { get; private set; }
            internal (
                string RuleName,
                Func<
                (
                    XElement FirstElement,
                    XElement SecondElement
                ),
                bool>[] Comparers 
            )[] Rules{ get; private set; }
            internal IEnumerable<(string,string)> ExpectedXPaths { get; private set; }
            internal string Message { get; private set; }

            internal static SimilarTestDataClass Create(
                string xmlString,
                (
                    string ruleName,
                    Func<
                        (
                            XElement FirstElement,
                            XElement SecondElement
                        ),
                        bool>[] comparers
                )[] rules,
                IEnumerable<(string,string)> expectedXPaths,
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
                (
                    string ruleName,
                    Func<
                        (
                            XElement FirstElement,
                            XElement SecondElement
                        ),
                        bool>[] comparers
                )[] rules,
                IEnumerable<(string,string)> expectedXpaths,
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
