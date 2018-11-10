using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml.Linq;

namespace LehmanLaidun.FileSystem.Test
{
    [TestClass]
    public class SimilarTest
    {
        [TestMethod]
        public void CreateWithElementAndXPathShouldCreateObject()
        {
            //  #   Arrange.
            var firstElement = new XElement("Name1", new[] { new XAttribute("A", "1") });
            var firstXpath = "//root/Name1[@A='1']";
            var secondElement = new XElement("Name2", new[] { new XAttribute("B", "2"), new XAttribute("C", "3") });
            var secondXpath = "//root/AnElement/Name2[@B='2' and @C='3']";

            //  #   Act.
            var res = Similar.Create(firstElement, firstXpath, secondElement, secondXpath);

            //  #   Assert.
            res.FirstElement.Should().Be(new XElement("Name1", new[] { new XAttribute("A", "1") }));
            res.FirstXpath.Should().Be("//root/Name1[@A='1']");
            res.SecondElement.Should().Be(new XElement("Name2", new[] { new XAttribute("B", "2"), new XAttribute("C", "3") }));
            res.SecondXpath.Should().Be("//root/AnElement/Name2[@B='2' and @C='3']");
        }

        [TestMethod]
        public void CreateWithElementAndXPathShouldNotCreateIfElementDoesNotMatchXPath()
        {
            //  #   Arrange.
            var firstElement = new XElement("Name1", new[] { new XAttribute("A", "1") });
            var firstXpath = "//root/Name1[@XXX='1']";
            var secondElement = new XElement("Name2", new[] { new XAttribute("B", "2"), new XAttribute("C", "3") });
            var secondXpath = "//root/AnElement/Name2[@B='2' and @C='3']";

            //  #   Act.
            ; try
            {
                Similar.Create(firstElement, firstXpath, secondElement, secondXpath);

                //  #   Assert.
                Assert.Fail("An exception should ha;ve been thrown but was not.");
            }
            catch (FirstElementAndXpathDoNotMatchException exc)
            {
                //  It should come here.
                Assert.IsTrue(true, "It should throw and exception.");

                exc.Element.Should().Be(firstElement.ToString());
                exc.Xpath.Should().Be(firstXpath);
                exc.Message.Should().Contain("A");
                exc.Message.Should().Contain("XXX");
            }

            //  #   Arrange.
            firstElement = new XElement("Name1", new[] { new XAttribute("A", "1") });
            firstXpath = "//root/Name1[@A='1']";
            secondElement = new XElement("Name2", new[] { new XAttribute("B", "2"), new XAttribute("C", "3") });
            secondXpath = "//root/AnElement/XXX[@B='2' and @C='3']";

            //  #   Act.
            try
            {
                Similar.Create(firstElement, firstXpath, secondElement, secondXpath);

                //  #   Assert.
                Assert.Fail("An exception should ha;ve been thrown but was not.");
            }
            catch (SecondElementAndXpathDoNotMatchException exc)
            {
                //  It should come here.
                Assert.IsTrue(true, "It should throw and exception.");

                exc.Message.Should().Contain("Name2");
                exc.Message.Should().Contain("XXX");
            }
        }

        [TestMethod]
        public void CreateWithXPathShouldCreateObject()
        {
            //  #   Arrange.
            var firstXpath = "//root/Name1[@A='1']";
            var secondXpath = "//root/AnElement/Name2[@B='2' and @C='3']";

            //  #   Act.
            var res = Similar.Create(firstXpath, secondXpath);

            //  #   Assert.
            res.FirstElement.Should().Be(new XElement("Name1", new[] { new XAttribute("A", "1") }));
            res.FirstXpath.Should().Be("//root/Name1[@A='1']");
            res.SecondElement.Should().Be(new XElement("Name2", new[] { new XAttribute("B", "2"), new XAttribute("C", "3") }));
            res.SecondXpath.Should().Be("//root/AnElement/Name2[@B='2' and @C='3']");
        }
    }
}
