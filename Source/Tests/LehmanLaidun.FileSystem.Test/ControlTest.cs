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
        public void CanCompareXml_ReturnNotEqualAndDIfferences()
        {
            foreach (var testDatum in CanCompareXmlTestData)
            {
                //  #   Act.
                var res = Logic.CompareXml(XDocument.Parse(testDatum.FirstXml), XDocument.Parse(testDatum.SecondXml));

                //  #   Assert.
                res.Result.Should().BeFalse("The comparision should have failed." + testDatum.Message);
                Assert_Differences(res.Differences, testDatum);

            }
        }

        [TestMethod]
        [DataRow(@"<root/>",
            new object[0],
            "No duplicate in simple root.")]
        [DataRow(@"
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
            new object[]
            {
                new []{
                    @"<f n='a'/>",
                    @"/root/d[@n='a']/f[@n='a']",
                    @"/root/d[@n='b']/f[@n='a']",
                }
            },
            "Duplicate files found in sibling directories.")]
        [DataRow(@"
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
            new object[]
            {
                new[]
                {
                    @"<f n='a'/>",
                    @"/root/d[@n='a']/f[@n='a']",
                    @"/root/d[@n='a']/d[@n='b']/f[@n='a']",
                }
            },
            "Duplicate files found in parent/child directories.")]
        public void CanFindDuplicates_ReturnAllDuplicates(string xmlString, object[] expecteds, string message)
        {
            //  #   Arrange.
            var doc = XDocument.Parse(xmlString);
            Func<IEnumerable<string>, (XElement element, IEnumerable<string> xpaths)> getElementAndSXpaths = stringList =>
             {
                 var element = XDocument.Parse(stringList.First()).Root;
                 var xpaths = stringList.Skip(1);
                 return (element: element, xpaths: xpaths);
             };

            //  #   Act.
            var res = Logic.FindDuplicates(doc);
            
            //  #   Assert.
            var expectedDuplicates = expecteds
                .Select(expected =>
                {
                    (var element, var xpaths) = getElementAndSXpaths((string[])expected);
                    return Duplicate.Create(element, xpaths);
                });

            res.Should().BeEquivalentTo(expectedDuplicates, message);
        }

        [DataTestMethod]
        [DynamicData(nameof(SimilarTestData))]
        public void CanFindSimilars_ReturnFittingSimilars(
            XDocument doc,
            (
                string RuleName,
                Logic.ComparerDelegate[] Comparers
            )[] rules,
            IEnumerable<(string, string)> expectedXpaths,
            string message)
        {
            //  #   Arrange.
            Func<string, XElement> toElement = (string xpath) =>
             {
                 var elementString = xpath.Split('/').Last();
                 var matches = Regex.Match(elementString, @"(.*)\[(.*)\]");
                 var name = matches.Groups[1].Value;
                 var attributesString = matches.Groups[2].Value;
                 var attributesStrings = attributesString.Split("and");
                 var attributes = attributesStrings
                    .Select(x =>
                    {
var nameValuePair = x.Split("=");
                        return (
                            name: nameValuePair.First().Trim().TrimStart('@').Trim(),
                            value: nameValuePair.Last().Trim().TrimStart('\'').TrimEnd('\'')
                        );
                    });
                 var ret = new XElement(
name,
attributes.Select(a => new XAttribute(a.name, a.value)));
                 return ret;
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

            Assert.Fail("TBA:Refine with type of similarity and also multiple in result.");
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

        private static void Assert_Differences(IEnumerable<Difference> actualDifferences, CanCompareXmlTestDataClass expectedTestDatum)
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
