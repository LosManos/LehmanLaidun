using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace LehmanLaidun.FileSystem.Test
{
    [TestClass]
    public partial class ControlTest
    {
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
        [DynamicData(nameof(CanCompareXmlTestData))]
        public void CanCompareXml_ReturnNotEqualAndDIfferences(
            string firstXml, 
            string secondXml, 
            IEnumerable<Difference>differences, 
            string message)
        {
            //  #   Act.
            var res = Logic.CompareXml(XDocument.Parse(firstXml), XDocument.Parse(secondXml));

            //  #   Assert.
            res.Result.Should().BeFalse("The comparision should have failed." + message);
            Assert_Differences(res.Differences, CanCompareXmlTestDataClass.Create(message, firstXml, secondXml, differences.ToArray()));
        }

        [TestMethod]
        [DynamicData(nameof(DuplicateTestData))]
        public void CanFindDuplicates_ReturnAllDuplicates(
            string xmlString, 
            DuplicateTestDataClass.ElementAndXPaths[] expecteds, 
            string message)
        {
            //  #   Arrange.
            var doc = XDocument.Parse(xmlString);

            //  #   Act.
            var res = Logic.FindDuplicates(doc);
            
            //  #   Assert.
            var expectedDuplicates = expecteds
                .Select(expected =>
                {
                    var element = XDocument.Parse(expected.ElementString).Root;
                    return Duplicate.Create(element, expected.Xpaths);
                });

            res.Should().BeEquivalentTo(expectedDuplicates, message);
        }

        [DataTestMethod]
        [DynamicData(nameof(SimilarTestData))]
        public void CanFindSimilars_ReturnFittingSimilars(
            XDocument doc,
            IEnumerable<Logic.Rule> rules,
            IEnumerable<(string, string)> expectedXpaths,
            string message)
        {
            //  #   Arrange.
            Func<string, XElement> toElement = (string xpath) =>
             {
                 var lastElementString = xpath.Split('/').Last();
                 var matches = Regex.Match(lastElementString, @"(.*)\[(.*)\]");
                 var name = matches.Groups[1].Value;
                 var attributes = matches.Groups[2].Value.Split("and")
                    .Select(x =>
                    {
                        var nameValuePair = x.Split("=");
                        return (
                            name: nameValuePair.First().Trim().TrimStart('@').Trim(),
                            value: nameValuePair.Last().Trim().TrimStart('\'').TrimEnd('\'')
                        );
                    });
                 return new XElement(
                    name,
                    attributes.Select(a => new XAttribute(a.name, a.value)));
             };
            Func<IEnumerable<(string FirstXpath, string SecondXpath)>, IEnumerable<Similar>> toSimilars =xpaths =>
            {
                return xpaths
                    .Select(ex => Similar.Create(
                        toElement(ex.FirstXpath),
                        ex.FirstXpath,
                        toElement(ex.SecondXpath),
                        ex.SecondXpath)
                    );
            };

            //  #   Act.
            var res = Logic.FindSimilars(doc, rules);

            //  #   Assert.
            res.Should().BeEquivalentTo(toSimilars(expectedXpaths), message);
        }

        [TestMethod]
        public void CanReturnListWithAllPropertiesSet()
        {
            //  #   Arrange.
            const string Path = @"c:\images";
            var files = new[] {
                new { pathfile = @"c:\images\20180924\image1.jpg", length = 3},
                new { pathfile = @"c:\images\20180922\image3.jpg", length = 5},
                new { pathfile = @"c:\images\image2.jpg", length = 12}
            };

            var mockedFileSystem = new MockFileSystem(
                files.ToDictionary(pf => pf.pathfile, pf => CreateMockFileData(pf.length))
            );

            var sut = LogicFactory.CreateForPath(mockedFileSystem, Path);

            //  #   Act.
            var res = sut.AsEnumerableFiles();

            //  #   Assert.
            res.Should().BeEquivalentTo(
                files.Select(f => CreateFileItem(f.pathfile, f.length))
            );
        }

        [TestMethod]
        public void CanReturnXmlWithAllPropertiesSet()
        {
            //  #   Arrange.
            const string Path = @"c:\images";
            var files = new[]
            {
                new { pathfile = @"c:\images\Vacation\20180606-100404.jpg", length = 15 },
                new { pathfile = @"c:\images\2018\201809\20180925-220604.jpg", length = 2 },
                new { pathfile = @"c:\images\2018\201809\20180925-220502.jpg", length = 4 },
                new { pathfile = @"c:\images\iphone backup\20180925-2207.jpg", length = 3 },
                new { pathfile = @"c:\images\stray image.jpg", length = 4 }
            };

            var mockedFileSystem = new MockFileSystem(
                files.ToDictionary(pf => pf.pathfile, pf => CreateMockFileData(pf.length))
            );

            var sut = LogicFactory.CreateForPath(mockedFileSystem, Path);

            //  #   Act.
            var res = sut.AsXDocument();

            //  #   Assert.
            res.Should().BeEquivalentTo(
                XDocument.Parse(@"
<root path='c:\images'>
    <directory path=''>
        <file name='stray image.jpg' length='4'/>
    </directory>
    <directory path='Vacation'>
        <file name='20180606-100404.jpg' length='15'/>
    </directory>
    <directory path='2018'>
        <directory path='201809'>
            <file name='20180925-220604.jpg' length='2'/>
            <file name='20180925-220502.jpg' length='4'/>
        </directory>
    </directory>
    <directory path='iphone backup'>
        <file name='20180925-2207.jpg' length='3'/>
    </directory>
</root>
"));
        }

        [DataTestMethod]
        [DataRow(@"
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

        private static void Assert_Differences
            (IEnumerable<Difference> actualDifferences, 
            CanCompareXmlTestDataClass expectedTestDatum)
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

        private static FileItem CreateFileItem(string pathfile, int length)
        {
            var fs = new MockFileSystem();
            var mf = new MockFileData(new string('a', length));
            //var fi = new MockFileInfo(fs, pathfile);
            fs.AddFile(pathfile, mf);
            return FileItem.Create(fs, pathfile);
        }

        private static MockFileData CreateMockFileData(int length /*Should really be long*/)
        {
            return new MockFileData(new string('a', length));
        }

        #endregion
    }
}
