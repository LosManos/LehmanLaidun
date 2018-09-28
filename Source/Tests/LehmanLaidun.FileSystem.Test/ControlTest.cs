using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Xml.Linq;

namespace LehmanLaidun.FileSystem.Test
{
    [TestClass]
    public class ControlTest
    {
        [DataTestMethod]
        [DataRow(@"<root/>", @"<root/>")]
        [DataRow(@"<root/>", @"<root><root/>")]
        [DataRow(@"
<root>
    <directory/>
</root>", @"
<root>
    <directory></directory>
</root>"
            )]
        [DataRow(@"
<root>
    <a/>
    <b/>
</root>", @"
<root>
    <b/>
    <a/>
</root>"
            )]
        [DataRow(@"
<root>
    <directory name='a' suffix='b'/>
</root>", @"
<root>
    <directory suffix='b' name='a'/>
</root>"
            )]
        public void CanCompareXml_ReturnEqualAndNoDifferenceForSameStrucure(string xml1, string xml2)
        {
            //  #   Act.
            var res = Logic.CompareXml(XDocument.Parse(xml1), XDocument.Parse(xml2));

            //  #   Assert.
            res.Result.Should().BeTrue("The XMLs are of equal strucure.");
            res.Differences.Should().BeEmpty();
        }

        [DataTestMethod]
        public void CanCompareXml_ReturnNotEqualAndDIfferences()
        {
            Assert.Fail("TBA");
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
            var res = Logic.SortXml(XDocument.Parse(source));

            //  #   Assert.
            res.Should().BeEquivalentTo(XDocument.Parse(expectedResult), message);
        }
    }
}
