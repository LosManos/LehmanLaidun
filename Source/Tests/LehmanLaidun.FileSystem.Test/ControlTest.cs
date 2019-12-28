using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Xml.Linq;

namespace LehmanLaidun.FileSystem.Test
{
    [TestClass]
    public partial class ControlTest
    {
        [DataTestMethod]
        [DynamicData(nameof(CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucureTestData))]
        public void CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucure(string xml1, string xml2, string message)
        {
            //  #   Act.
            var res = Logic.CompareXml(XDocument.Parse(xml1), XDocument.Parse(xml2));

            //  #   Assert.
            res.Result.Should().BeTrue("The XMLs are of equal strucure." + message);
            res.Differences.Should().BeEmpty();
        }

        [TestMethod]
        [DynamicData(nameof(CanCompareXml_ReturnNotEqualAndDIfferencesTestData))]
        public void CanCompareXml_ReturnNotEqualAndDIfferences(
            string firstXml,
            string secondXml,
            IEnumerable<Difference> differences,
            string message)
        {
            //  #   Act.
            var res = Logic.CompareXml(XDocument.Parse(firstXml), XDocument.Parse(secondXml));

            //  #   Assert.
            res.Result.Should().BeFalse("The comparision should have failed." + message);
            Assert_Differences(res.Differences, CanCompareXml_ReturnNotEqualAndDIfferencesTestDataClass.Create(message, firstXml, secondXml, differences.ToArray()));
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
            IEnumerable<Similar> expecteds,
            string message)
        {
            //  #   Act.
            var res = Logic.FindSimilars(doc, rules);

            //  #   Assert.
            res.Should().BeEquivalentTo(expecteds, message);
        }

        [TestMethod]
        public void CanReturnListWithAllPropertiesSet()
        {
            //  #   Arrange.
            const string Path = @"c:\images";
            var files = new[] {
                new { pathfile = @"c:\images\20180924\image1.jpg", length = 3, lastAccessTime = DateTime.Parse("2010-01-04 11:22:33Z") },
                new { pathfile = @"c:\images\20180922\image3.jpg", length = 5, lastAccessTime = DateTime.Parse("2010-01-06 11:22:33Z") },
                new { pathfile = @"c:\images\image2.jpg", length = 12, lastAccessTime = DateTime.Parse("2010-01-13 11:22:33Z") }
            };

            var mockedFileSystem = new MockFileSystem(
                files.ToDictionary(pf => pf.pathfile, pf => CreateMockFileData(pf.length, pf.lastAccessTime))
            );

            var sut = LogicFactory.CreateForPath(mockedFileSystem, Path);

            //  #   Act.
            var res = sut.AsEnumerableFiles();

            //  #   Assert.
            res.Should().BeEquivalentTo(
                files.Select(f => CreateFileItem(f.pathfile, f.length, f.lastAccessTime))
            );
        }

        [TestMethod]
        public void CanReturnWithAllPropertiesSet()
        {
            const string Path = @"c:\images";
            var files = new[]
            {
                new { 
                    pathfile = @"c:\images\Vacation\20180606-100404.jpg", 
                    length = 15, 
                    lastAccessTime = CreateAsUtc("2010-01-16 11:16:33Z") },
            };

            var mockedFileSystem = new MockFileSystem(
                files.ToDictionary(pf => pf.pathfile, pf => CreateMockFileData(pf.length, pf.lastAccessTime))
            );

            var sut = LogicFactory.CreateForPath(mockedFileSystem, Path);

            //  #   Act.
            var res = sut.AsXDocument();

            //  #   Assert.
            res.Should().BeEquivalentTo(
                XDocument.Parse(@"
<root path='c:\images'>
    <directory name='Vacation'>
        <file name='20180606-100404.jpg' length='15' lastWriteTime='2010-01-16T11:16:33.0000000Z'/>
    </directory>
</root>
"));
        }

        /// <summary>This test is a kludge as it tests both the output structure
        /// and its contents and anything between.
        /// Feel free to rewrite to (a) cleaner test(s).
        /// </summary>
        [TestMethod]
        public void CanReturnXml()
        {
            const string Path = @"c:\images";
            var files = new[]
            {
                new { pathfile = @"c:\images\Vacation\20180606-100404.jpg", length = 15, lastAccessTime = CreateAsUtc("2010-01-16 11:16:33Z") },
                new { pathfile = @"c:\images\2018\201809\20180925-220604.jpg", length = 2, lastAccessTime = CreateAsUtc("2010-01-03 11:03:33Z") },
                new { pathfile = @"c:\images\2018\201809\20180925-220502.jpg", length = 4, lastAccessTime = CreateAsUtc("2010-01-05 11:05:33Z") },
                new { pathfile = @"c:\images\iphone backup\20180925-2207.jpg", length = 3, lastAccessTime = CreateAsUtc("2010-01-04 11:04:33Z") },
                new { pathfile = @"c:\images\stray image.jpg", length = 4, lastAccessTime = CreateAsUtc("2010-01-05 11:05:34Z") }
            };

            var mockedFileSystem = new MockFileSystem(
                files.ToDictionary(pf => pf.pathfile, pf => CreateMockFileData(pf.length, pf.lastAccessTime))
            );

            var sut = LogicFactory.CreateForPath(mockedFileSystem, Path);

            //  #   Act.
            var res = sut.AsXDocument();

            //  #   Assert.
            res.Should().BeEquivalentTo(
                XDocument.Parse(@"
<root path='c:\images'>
    <directory name=''>
        <file name='stray image.jpg' length='4' lastWriteTime='2010-01-05T11:05:34.0000000Z'/>
    </directory>
    <directory name='Vacation'>
        <file name='20180606-100404.jpg' length='15' lastWriteTime='2010-01-16T11:16:33.0000000Z'/>
    </directory>
    <directory name='2018'>
        <directory name='201809'>
            <file name='20180925-220604.jpg' length='2' lastWriteTime='2010-01-03T11:03:33.0000000Z'/>
            <file name='20180925-220502.jpg' length='4' lastWriteTime='2010-01-05T11:05:33.0000000Z'/>
        </directory>
    </directory>
    <directory name='iphone backup'>
        <file name='20180925-2207.jpg' length='3' lastWriteTime='2010-01-04T11:04:33.0000000Z'/>
    </directory>
</root>
"));
        }

        [DataTestMethod]
        [DynamicData(nameof(SortTestData))]
        public void CanSortXml(string source, string expectedResult, string message)
        {
            //  #   Act.
            var res = Logic.UT_SortXml(XDocument.Parse(source));

            //  #   Assert.
            res.Should().BeEquivalentTo(XDocument.Parse(expectedResult), message);
        }

        [TestMethod]
        public void RootNameIsKnown()
        {
            //  #   Arrange.
            var rootName = Logic.ElementNameRoot;
            const string Path = @"c:\images";
            var files = new[]
            {
                new { pathfile = @"c:\images\whatever.jpg", length = 4, lastAccessTime = DateTime.Parse("2010-01-05 11:22:33Z")  }
            };

            var mockedFileSystem = new MockFileSystem(
                files.ToDictionary(pf => pf.pathfile, pf => CreateMockFileData(pf.length, pf.lastAccessTime))
            );

            var sut = LogicFactory.CreateForPath(mockedFileSystem, Path);

            //  #   Act.
            var res = sut.AsXDocument();

            //  #   Assert.
            res.Root.Name.LocalName.Should().Be(rootName);
        }

        #region Helper methods.

        private static void Assert_Differences
            (IEnumerable<Difference> actualDifferences,
            CanCompareXml_ReturnNotEqualAndDIfferencesTestDataClass expectedTestDatum)
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

        private static FileItem CreateFileItem(string pathfile, int length /*Should really be a long.*/, DateTime lastWriteTime)
        {
            var fs = new MockFileSystem();
            fs.AddFile(pathfile, CreateMockFileData(length, lastWriteTime));
            return FileItem.Create(fs, pathfile);
        }

        private static MockFileData CreateMockFileData(
            int length, /*Should really be long*/
            DateTime lastWriteTime)
        {
            var ret = new MockFileData(new string('a', length));
            ret.LastWriteTime = lastWriteTime;
            return ret;
        }

        /// <summary>This helper method parses the parameter 
        /// and returns a datetime in UTC.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static DateTime CreateAsUtc(string s)
        {
            return DateTime.Parse(s, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
        }

    #endregion
}
}
