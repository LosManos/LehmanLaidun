using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Xml.Linq;
using static LehmanLaidun.FileSystem.Difference;

namespace LehmanLaidun.FileSystem.Test
{
    [TestClass]
    public class ControlTest
    {
        public class TestDatum
        {
            public string FirstXml { get; }
            public string SecondXml { get; }
            public IEnumerable<Difference> Differences { get; }
            public string Message { get; }

            [Obsolete("Replace with the one taking [message] as first argument", false)]
            public TestDatum(string firstXml, string secondXml, Difference difference, string message)
                : this(firstXml, secondXml, new[] { difference }, message)
            {
            }

            [Obsolete("Replace with the one taking [message] as first argument", false)]
            public TestDatum(string firstXml, string secondXml, IEnumerable<Difference> differences, string message)
            {
                FirstXml = firstXml;
                SecondXml = secondXml;
                Differences = differences;
                Message = message;
            }

            public TestDatum(
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

        IEnumerable<TestDatum> _canCompareXml_ReturnNotEqualAndDIfferences_TestData
        {
            get
            {
                // Indata with stuff found only in the *first* tree.
                //
                yield return new TestDatum(
                    "The element should be found only in the first tree.", 
                    "<root><a/></root>",
                    "<root></root>",
                    Difference.Create(new XElement("a"), "/root/a[not(@*)]", FoundOnlyIn.First)
                );
                yield return new TestDatum(
                    "The inner element should be found only in the first tree.",
                    "<root><a><b/></a></root>",
                    "<root><a/></root>",
                    Difference.Create(new XElement("b"), "/root/a[not(@*)]/b[not(@*)]",FoundOnlyIn.First)
                );
                yield return new TestDatum(
                    "Element with attributes differs from one without. The attributes are in the first tree.",
                    "<root><a b='c'/></root>",
                    "<root><a/></root>",
                    Difference.Create(new XElement("a", new XAttribute("b", "c")), "/root/a[@b='c']", FoundOnlyIn.First),
                    Difference.Create(new XElement("a"), "/root/a[not(@*)]", FoundOnlyIn.Second)
                );
                yield return new TestDatum(
                    "The sequence elements should be found only in the first tree.",
                    "<root><a/><b/></root>",
                    "<root></root>",
                        Difference.Create(new XElement("a")  , "/root/a[not(@*)]", FoundOnlyIn.First),
                        Difference.Create(new XElement("b")  , "/root/b[not(@*)]", FoundOnlyIn.First)
                );

                //  Testdata with stuff found only in the *second* tree.
                //
                yield return new TestDatum(
                    "The element should be found only in the second tree.",
                    "<root></root>",
                    "<root><a/></root>",
                    Difference.Create(new XElement("a"), "/root/a[not(@*)]", FoundOnlyIn.Second)
                );
                yield return new TestDatum(
                    "The inner element should be found only in the second tree.",
                    "<root><a/></root>",
                    "<root><a><b/></a></root>",
                    Difference.Create(new XElement("b"), "/root/a[not(@*)]/b[not(@*)]", FoundOnlyIn.Second)
                    );
                yield return new TestDatum(
                    "The element with attributes should be found only in the second tree.",
                    "<root><a/></root>",
                    "<root><a b='c'/></root>",
                    Difference.Create(new XElement("a"), "/root/a[not(@*)]", FoundOnlyIn.First),
                    Difference.Create(new XElement("a", new XAttribute("b", "c")), "/root/a[@b='c']", FoundOnlyIn.Second)
                );
                yield return new TestDatum(
                    "The elements should be found only in the second tree.",
                    "<root></root>",
                    "<root><a/><b/></root>",
                    Difference.Create(new XElement("a")  , "/root/a[not(@*)]", FoundOnlyIn.Second),
                    Difference.Create(new XElement("b")  , "/root/b[not(@*)]", FoundOnlyIn.Second)
                );
                yield return new TestDatum(
                    "Elements with only attributes differing should be found.",
                    "<root><a b='c'/></root>",
                    "<root><a d='e'/></root>",
                    Difference.Create(new XElement("a", new XAttribute("b", "c")), "/root/a[@b='c']", FoundOnlyIn.First),
                    Difference.Create(new XElement("a", new XAttribute("d", "e")), "/root/a[@d='e']", FoundOnlyIn.Second)
                );
            }
        }

        [DataTestMethod]
        [DataRow(@"<root/>", @"<root/>", "A simple root should be equal to itself.")]
        [DataRow(@"<root a='b'/>", @"<root/>", "The attributes of the root does not matter.")]
        [DataRow(@"<root/>", @"<root></root>", "Elements can be both simple and complex. (what is it called?)")]
        [DataRow(@"
<root>
    <directory/>;
</root>", @"
<root>
    <directory></directory>
</root>",
            "Sub-root element can be both simple and complex. (what is it called?)"
            )]
        [DataRow(@"
<root>
    <a/>
    <b/>
</root>", @"
<root>
    <b/>
    <a/>
</root>",
            "The ordering of the elements does not care."
            )]
        [DataRow(@"
<root>
    <a b='c' d='e'/>
</root>", @"
<root>
    <a d='e' b='c'/>
</root>",
            "The ordering of the attributes does note care."
            )]
        [DataRow(@"
<root>
    <a/>
    <b c='d' e='f'/>
</root>", @"
<root>
    <b e='f' c='d'/>
    <a/>
</root>",
            "Neither the ordering of the elements nore the attributes care."
            )]
        [DataRow(@"
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
            "The ordering of sub elements does not care.")]
        public void CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucure(string xml1, string xml2, string message)
        {
            //  #   Act.
            var res = Logic.CompareXml(XDocument.Parse(xml1), XDocument.Parse(xml2));

            //  #   Assert.
            res.Result.Should().BeTrue("The XMLs are of equal strucure." + message);
            res.Differences.Should().BeEmpty();
        }

        [TestMethod]
        public void CanCompareXml_ReturnNotEqualAndDIfferences()
        {
            foreach (var testDatum in _canCompareXml_ReturnNotEqualAndDIfferences_TestData)
            {
                //  #   Act.
                var res = Logic.CompareXml(XDocument.Parse(testDatum.FirstXml), XDocument.Parse(testDatum.SecondXml));

                //  #   Assert.
                res.Result.Should().BeFalse("The comparision should have failed." + testDatum.Message);
                Assert_Differences(res.Differences, testDatum);

            }
        }

        public class TempErrorRow
        {
            public string Messsage { get; set; }
        }

        [TestMethod]
        public void CanReturnList()
        {
            const string Path = @"c:\images";
            var paths = new[] {
                @"c:\images\20180924\image1.jpg",
                @"c:\images\20180922\image3.jpg",
                @"c:\images\image2.jpg" };

            var mockedFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            paths.ToList().ForEach(p => mockedFileSystem.AddFile(p, new MockFileData("a")));

            var sut = LogicFactory.CreateForPath(mockedFileSystem, Path);

            var res = sut.AsEnumerableFiles();

            res.Select(r => System.IO.Path.Combine(r.Path, r.Name))
                .Should().Contain(paths,
                "Every file should be there in any order.");
            res.Count().Should().Be(3, "Everything should be accounted for.");
        }

        [TestMethod]
        public void CanReturnXml()
        {
            //  #   Arrange.
            const string Path = @"c:\images";
            var paths = new[]
            {
                @"c:\images\Vacation\20180606-100404.jpg",
                @"c:\images\2018\201809\20180925-220604.jpg",
                @"c:\images\2018\201809\20180925-220502.jpg",
                @"c:\images\iphone backup\20180925-2207.jpg",
                @"c:\images\stray image.jpg"
            };
            var mockedFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            paths.ToList().ForEach(p => mockedFileSystem.AddFile(p, new MockFileData("a")));

            var sut = LogicFactory.CreateForPath(mockedFileSystem, Path);

            //  #   Act.
            var res = sut.AsXDocument();

            //  #   Assert.
            res.Should().BeEquivalentTo(
                XDocument.Parse(@"
<root path='c:\images'>
    <directory path=''>
        <file name='stray image.jpg'/>
    </directory>
    <directory path='Vacation'>
        <file name='20180606-100404.jpg'/>
    </directory>
    <directory path='2018'>
        <directory path='201809'>
            <file name='20180925-220604.jpg'/>
            <file name='20180925-220502.jpg'/>
        </directory>
    </directory>
    <directory path='iphone backup'>
        <file name='20180925-2207.jpg'/>
    </directory>
</root>
"));
        }

        [DataTestMethod]
        [DataRow(@"
            <root>
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
        )]
        [DataRow(@"
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
        )]
        [DataRow(@"
            <root>
                <a delta='a' camma='a'/>
                <a beta='a' alfa='a'/>
            </root>", @"
            <root>
                <a alfa='a' beta='a'/>
                <a camma='a' delta='a'/>
            </root>",
            "Sorting attributes."
        )]
        [DataRow(@"
            <root>
                <a bb='' c=''/>
                <a b='' bc=''/>
            </root>", @"
            <root>
                <a b='' bc=''/>
                <a bb='' c=''/>
            </root>",
            "Sorting elements by attributes."
        )]
        public void CanSortXml(string source, string expectedResult, string message)
        {
            //  #   Act.
            var res = Logic.UT_SortXml(XDocument.Parse(source));

            //  #   Assert.
            res.Should().BeEquivalentTo(XDocument.Parse(expectedResult), message);
        }

        #region Helper methods.

        private static void Assert_Differences(IEnumerable<Difference> actualDifferences, TestDatum expectedTestDatum)
        {
            foreach (var diff in actualDifferences)
            {
                var matchingDifference = expectedTestDatum.Differences.SingleOrDefault(d =>
                   diff.FirstElement?.ToString() == d.FirstElement?.ToString() &&
                   diff.FirstXPath == d.FirstXPath &&
                   diff.SecondElement?.ToString() == d.SecondElement?.ToString() &&
                   diff.SecondXPath == d.SecondXPath);
                if (matchingDifference == null)
                {
                    Assert.Fail(
                        "Expected Difference was not found." + Environment.NewLine +
                        expectedTestDatum.Message + Environment.NewLine + Environment.NewLine +
                        $"Indata:{Environment.NewLine}" +
                        $"First Xml:{expectedTestDatum.FirstXml}{Environment.NewLine}" +
                        $"Second Xml:{expectedTestDatum.SecondXml}{Environment.NewLine}" +
                        $"{Environment.NewLine}" +
                        $"All expected differences:{Environment.NewLine}" +
                        expectedTestDatum.Differences.Select(d =>
                            $"First Element:{d.FirstElement}{Environment.NewLine}" +
                            $"First XPath: {d.FirstXPath}{Environment.NewLine}" +
                            $"Second Element:{d.SecondElement}{Environment.NewLine}" +
                            $"Second XPath:{d.SecondXPath}{Environment.NewLine}"
                        ).StringJoin(Environment.NewLine) +
                        $"{Environment.NewLine}" +
                        $"Actual outcome:{Environment.NewLine}" +
                        $"First Element:{diff.FirstElement}{Environment.NewLine}" +
                        $"First XPath: {diff.FirstXPath}{Environment.NewLine}" +
                        $"Second Element:{diff.SecondElement}{Environment.NewLine}" +
                        $"Second XPath:{diff.SecondXPath}{Environment.NewLine}"
                    );
                }
            }
        }

        #endregion
    }
}
