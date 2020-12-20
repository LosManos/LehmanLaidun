using System.Collections.Generic;
using System.Xml.Linq;

namespace LehmanLaidun.FileSystem.Test
{
    partial class ControlTest
    {
        private static IEnumerable<object[]> CanCompareXml_ReturnEqualAndNoDifferenceForSameStructureTestData
        {
            get
            {
                yield return CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestDataClass.Create(
                    "A simple root should be equal to itself.",
                    @"<root/>",
                    @"<root/>",
                    new string[0])
                    .ToObjectArray();

                yield return CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestDataClass.Create(
                    "The attributes of the root does not matter.",
                    @"<root a='b'/>",
                    @"<root/>",
                    new[] { "*" })
                    .ToObjectArray();

                yield return CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestDataClass.Create(
                    "Elements can be both simple and complex. (what is it called?)",
                    @"<root/>",
                    @"<root></root>",
                    new string[0])
                    .ToObjectArray();

                yield return CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestDataClass.Create(
                    "Sub-root element can be both simple and complex. (what is it called?)",
                    @"
                    <root>
                        <directory/>;
                    </root>", 
                    @"
                    <root>
                        <directory></directory>
                    </root>",
                    new string[0])
                    .ToObjectArray();

                yield return CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestDataClass.Create(
                    "The ordering of the elements does not care.",
                    @"
                    <root>
                        <a/>
                        <b/>
                    </root>", @"
                    <root>
                        <b/>
                        <a/>
                    </root>",
                    new string[0])
                    .ToObjectArray();

                yield return CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestDataClass.Create(
                    "The ordering of the attributes does not matter.",
                    @"
                    <root>
                        <a b='c' d='e'/>
                    </root>", @"
                    <root>
                        <a d='e' b='c'/>
                    </root>",
                    new[] { "*" })
                    .ToObjectArray();

                yield return CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestDataClass.Create(
                    "Neither the ordering of the elements nor the attributes care.",
                    @"
                    <root>
                        <a/>
                        <b c='d' e='f'/>
                    </root>", 
                    @"
                    <root>
                        <b e='f' c='d'/>
                        <a/>
                    </root>",
                    new[] { "*" })
                    .ToObjectArray();

                yield return CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestDataClass.Create(
                    "The ordering of sub elements does not care.",
                    @"
                <root>
                    <a>
                        <b/>
                    </a>
                    <c>
                        <d/>
                    </c>
                </root>", @"
                <root>
                    <c>
                        <d/>
                    </c>
                    <a>
                        <b/>
                    </a>
                </root>",
                    new string[0])
                .ToObjectArray();

                yield return CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestDataClass.Create(
                    "Only chosen attributes are compared",
                    @"<root><a b='c' d='e'/></root>",
                    @"<root><a b='d' d='e'/></root>",
                    new[] { "d" })
                .ToObjectArray();

                yield return CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestDataClass.Create(
                    "Some attributes should be included so the xmls should be equal.",
                    "<root><a b='c' d='e'/></root>",
                    "<root><a b='c' f='g'/></root>",
                    new[] { "b" })
                .ToObjectArray();

                yield return CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestDataClass.Create(
                    "No attributes should be included in the comparison",
                    "<root><a b='c'/></root>",
                    "<root><a d='e'/></root>",
                    new string[0])
                .ToObjectArray();
            }
        }

        private static IEnumerable<object[]> CanCompareXml_ReturnNotEqualAndDifferencesTestData
        {
            get
            {
                // Indata with stuff found only in the *first* tree.
                //
                yield return CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass.Create(
                    "The element should be found only in the first tree and we don't want any attribute at all",
                    "<root><a/></root>",
                    "<root></root>",
                    new[] {"&"},
                    Difference.Create(new XElement("a"), "/root/a[not(@*)]", Difference.FoundOnlyIn.First)
                ).ToObjectArray();
                yield return CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass.Create(
                    "The element should be found only in the first tree and we don't care about attributes",
                    "<root><a/></root>",
                    "<root></root>",
                    new string[0],
                    Difference.Create(new XElement("a"), "/root/a", Difference.FoundOnlyIn.First)
                ).ToObjectArray();
                yield return CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass.Create(
                    "The inner element should be found only in the first tree.",
                    "<root><a><b/></a></root>",
                    "<root><a/></root>",
                    new string[0],
                    Difference.Create(new XElement("b"), "/root/a/b", Difference.FoundOnlyIn.First)
                ).ToObjectArray();
                yield return CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass.Create(
                    "Element with attributes differs from one without. The attributes are in the first tree.",
                    "<root><a b='c'/></root>",
                    "<root><a/></root>",
                    new[] {"*"},
                    Difference.Create(new XElement("a", new XAttribute("b", "c")), "/root/a[@b='c']", Difference.FoundOnlyIn.First),
                    Difference.Create(new XElement("a"), "/root/a[not(@*)]", Difference.FoundOnlyIn.Second)
                ).ToObjectArray();
                yield return CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass.Create(
                    "The sequence elements should be found only in the first tree.",
                    "<root><a/><b/></root>",
                    "<root></root>",
                    new[] { "&" },
                        Difference.Create(new XElement("a"), "/root/a[not(@*)]", Difference.FoundOnlyIn.First),
                        Difference.Create(new XElement("b"), "/root/b[not(@*)]", Difference.FoundOnlyIn.First)
                ).ToObjectArray();

                //  Testdata with stuff found only in the *second* tree.
                //
                yield return CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass.Create(
                    "The element should be found only in the second tree.",
                    "<root></root>",
                    "<root><a/></root>",
                    new[] { "&" },
                    Difference.Create(new XElement("a"), "/root/a[not(@*)]", Difference.FoundOnlyIn.Second)
                ).ToObjectArray();
                yield return CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass.Create(
                    "The inner element should be found only in the second tree.",
                    "<root><a/></root>",
                    "<root><a><b/></a></root>",
                    new[] { "&" },
                    Difference.Create(new XElement("b"), "/root/a[not(@*)]/b[not(@*)]", Difference.FoundOnlyIn.Second)
                ).ToObjectArray();
                yield return CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass.Create(
                    "The element with attributes should be found only in the second tree.",
                    "<root><a/></root>",
                    "<root><a b='c'/></root>",
                    new[] { "b" },
                    Difference.Create(new XElement("a"), "/root/a[not(@*)]", Difference.FoundOnlyIn.First),
                    Difference.Create(new XElement("a", new XAttribute("b", "c")), "/root/a[@b='c']", Difference.FoundOnlyIn.Second)
                ).ToObjectArray();
                yield return CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass.Create(
                    "The elements should be found only in the second tree.",
                    "<root></root>",
                    "<root><a/><b/></root>",
                    new[] { "&" },
                    Difference.Create(new XElement("a"), "/root/a[not(@*)]", Difference.FoundOnlyIn.Second),
                    Difference.Create(new XElement("b"), "/root/b[not(@*)]", Difference.FoundOnlyIn.Second)
                ).ToObjectArray();
                yield return CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass.Create(
                    "Elements with only attributes differing should be found.",
                    "<root><a b='c'/></root>",
                    "<root><a d='e'/></root>",
                    new[] {"b", "d"},
                    Difference.Create(new XElement("a", new XAttribute("b", "c")), "/root/a[@b='c']", Difference.FoundOnlyIn.First),
                    Difference.Create(new XElement("a", new XAttribute("d", "e")), "/root/a[@d='e']", Difference.FoundOnlyIn.Second)
                ).ToObjectArray();

                //  Test data for verifying only certain attributes are compared.
                //
                yield return CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass.Create(
                    "All attributes should be included so the xmls should differ.",
                    "<root><a b='c' d='e'/></root>",
                    "<root><a b='c' f='g'/></root>",
                    new[] { "*" },
                    Difference.Create(new XElement("a",
                        new[] {
                            new XAttribute("b", "c"),
                            new XAttribute("d", "e")
                        }), "/root/a[@b='c' and @d='e']",
                        Difference.FoundOnlyIn.First),
                    Difference.Create(new XElement("a",
                        new[] {
                            new XAttribute("b", "c"),
                            new XAttribute("f", "g") 
                        }), "/root/a[@b='c' and @f='g']", 
                        Difference.FoundOnlyIn.Second)
                ).ToObjectArray();

                yield return CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass.Create(
                    "Some (i.e. all) attributes should be included so the xmls should differ.",
                    "<root><a b='c' d='e'/></root>",
                    "<root><a b='c' f='g'/></root>",
                    new[] { "b", "d", "f" },
                    Difference.Create(
                        new XElement("a", 
                        new[] {
                            new XAttribute("b", "c"),
                            new XAttribute("d", "e")
                        }), "/root/a[@b='c' and @d='e']", 
                        Difference.FoundOnlyIn.First),
                    Difference.Create(new XElement("a", 
                        new[]{
                            new XAttribute("b","c"),
                            new XAttribute("f", "g")
                        }), "/root/a[@b='c' and @f='g']", 
                        Difference.FoundOnlyIn.Second)
                ).ToObjectArray();
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
                    new Similar[] { },
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
                                    firstElement.Attribute("Name")!.Value ==
                                    secondElement.Attribute("Name")!.Value &&
                                    firstElement.Attribute("Size")!.Value !=
                                    secondElement.Attribute("Size")!.Value;
                            }
                        }
                    )   },
                    Similar.Create(
                        "Just a single arbitrary rule - Equal Name but different Size",
                        @"/root/d[@Name='a']/f[@Name='b' and @Size='12']",
                        @"/root/d[@Name='c']/f[@Name='b' and @Size='13']"
                    ),
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
                    Similar.Create(
                        "An arbitrary rule - any element with a n=b tag.",
                        @"/root/d[@n='a']/f[@n='c' and @s='123']",
                        @"/root/d[@n='c']/f[@n='c' and @s='13']"
                    ),
                    "Several rules should be executed."
                    ).ToObjectArray();

                yield return SimilarTestDataClass.Create(
                    @"
<root>
    <d n='a'>
        <f n='aa'/>
    </d>
    <d n='b'>
        <f n='aa'/>
        <f n='ba'/>
        <f n='ca'/>
    </d>
    <d n='c'>
        <f n='ca'/>
    </d>
</root>",
                new[]
                {
                    Logic.Rule.Create("Any A",
                        (e1, e2) => e1.Attribute("n")!.Value == "aa" && e2.Attribute("n")!.Value == "aa"),
                    Logic.Rule.Create("Any C",
                        (e1, e2) => e1.Attribute("n")!.Value=="ca" && e2.Attribute("n")!.Value == "ca")
                },
                new[] {
                Similar.Create(
                    "Any A", 
                    @"/root/d[@n='a']/f[@n='aa']", 
                    @"/root/d[@n='b']/f[@n='aa']"
                    ),
                Similar.Create(
                    "Any C", 
                    @"/root/d[@n='b']/f[@n='ca']", 
                    @"/root/d[@n='c']/f[@n='ca']"
                    ), },
                "Several similars from different rules"
                ).ToObjectArray();
            }
        }

        private static IEnumerable<object[]> SortTestData
        {
            get
            {
                yield return SortTestDataClass.Create(@"
                    <root>l
                        <b a='a'>
                            <bb a='a'/>
                        </b>
                        <a a='a'/>
                    </root>", @"
                    <root>
                        <a a='a'/>
                        <b a='a'>
                            <bb a='a'/>
                        </b>
                    </root>",
                    "Sorting elements."
                ).ToObjectArray();
                    yield return SortTestDataClass.Create(@"
                    <root>
                        <directory>
                            <b a='a'/>
                            <a a='a'/>
                        </directory>
                    </root>", @"
                    <root>
                        <directory>
                            <a a='a'/>
                            <b a='a'/>
                        </directory>
                    </root>",
                    "Sorting elements in descendant."
                ).ToObjectArray();
                    yield return SortTestDataClass.Create(@"
                    <root>
                        <a delta='a' camma='a'/>
                        <a beta='a' alfa='a'/>
                    </root>", @"
                    <root>
                        <a alfa='a' beta='a'/>
                        <a camma='a' delta='a'/>
                    </root>",
                    "Sorting attributes."
                ).ToObjectArray();
                    yield return SortTestDataClass.Create(@"
                    <root>
                        <a bb='' c=''/>
                        <a b='' bc=''/>
                    </root>", @"
                    <root>
                        <a b='' bc=''/>
                        <a bb='' c=''/>
                    </root>",
                    "Sorting elements by attributes."
                ).ToObjectArray();
            }
    }

        private class CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestDataClass
        {
            internal string Xml1 { get; }
            internal string Xml2 { get; }
            public IEnumerable<string> ComparedAttributeKeys { get; }
            internal string Message { get; }

            internal static CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestDataClass Create(string message, string xml1, string xml2, IEnumerable<string> comparedAttributeKeys)
            {
                return new CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestDataClass(xml1, xml2, comparedAttributeKeys, message);
            }

            private CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestDataClass(string xml1, string xml2, IEnumerable<string> comparedAttributeKeys, string message)
            {
                Xml1 = xml1;
                Xml2 = xml2;
                ComparedAttributeKeys = comparedAttributeKeys;
                Message = message;
            }

            internal object[] ToObjectArray()
            {
                return new object[]
                {
                    Xml1,
                    Xml2,
                    ComparedAttributeKeys,
                    Message
                };
            }

        }

        private class CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass
        {
            internal string FirstXml { get; }
            internal string SecondXml { get; }
            public IEnumerable<string> ComparedAttributeKeys { get; }
            internal IEnumerable<Difference> Differences { get; }
            internal string Message { get; }

            internal static CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass Create(
                string message,
                string firstXml,
                string secondXml,
                IEnumerable<string> comparedAttributeKeys,
                params Difference[] differences)
            {
                return new CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass(message, firstXml, secondXml, comparedAttributeKeys, differences);
            }

            private CanCompareXml_ReturnNotEqualAndDifferencesTestDataClass(
                string message,
                string firstXml,
                string secondXml,
                IEnumerable<string> comparedAttributeKeys,
                params Difference[] differences)
            {
                Message = message;
                FirstXml = firstXml;
                SecondXml = secondXml;
                ComparedAttributeKeys = comparedAttributeKeys;
                Differences = differences;
            }

            internal object[] ToObjectArray()
            {
                return new object[]
                {
                    FirstXml,
                    SecondXml,
                    ComparedAttributeKeys,
                    Differences,
                    Message
                };
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
            internal IEnumerable<Similar> Expecteds { get; private set; }
            internal string Message { get; private set; }

            internal static SimilarTestDataClass Create(
                string xmlString,
                Logic.Rule[] rules,
                Similar expecteds,
                string message
                )
            {
                return Create(xmlString, rules, new[] { expecteds }, message);
            }

            internal static SimilarTestDataClass Create(
                string xmlString,
                Logic.Rule[] rules,
                IEnumerable<Similar> expecteds,
                string message
                )
            {
                return new SimilarTestDataClass(
                    xmlString,
                    rules,
                    expecteds,
                    message);
            }
            
            internal object[] ToObjectArray()
            {
                return new object[]
                {
                    Xml,
                    Rules,
                    Expecteds,
                    Message
                };
            }

            private SimilarTestDataClass(
                    string xmlString,
                    Logic.Rule[] rules,
                    IEnumerable<Similar> expecteds,
                    string message)
            {
                Xml = XDocument.Parse(xmlString);
                Expecteds = expecteds;
                Rules = rules;
                Message = message;
            }
        }

        private class SortTestDataClass
        {
            internal string ExpectedResult { get; }
            internal string Message { get; }
            internal string XmlSource { get; }

            internal static SortTestDataClass Create(string xmlSource, string expectedResult, string message)
            {
                return new SortTestDataClass(xmlSource, expectedResult, message);
            }

            internal object[] ToObjectArray()
            {
                return new object[]
                {
                    XmlSource,
                    ExpectedResult,
                    Message
                };
            }

            private SortTestDataClass(string xmlSource, string expectedResult, string message)
            {
                XmlSource = xmlSource;
                ExpectedResult = expectedResult;
                Message = message;
            }
        }
    }
}
