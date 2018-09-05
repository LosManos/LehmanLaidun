using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace LehmanLaidun.FileSystem.Test
{
	[TestClass]
	public class TeeTest
	{
		[TestMethod]
		public void ShouldParse()
		{
			var input = @"
a
	b
	c
";

			var res = Node<string>.Parse(input, s => s);

            Assert.AreEqual(
                "root",
                res.Data);
            var childA = res.Children.Single();
			Assert.AreEqual(
				"a",
				childA.Data);
            Assert.AreEqual(
                2,
                childA.Children.Count);
            Assert.AreEqual(
                "b",
                childA.Children.First().Data);
            Assert.AreEqual(
                "c",
                childA.Children.Skip(1).First().Data);
		}
	}
}
