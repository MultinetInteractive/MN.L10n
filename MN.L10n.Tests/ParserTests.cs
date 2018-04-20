using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MN.L10n.Tests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void TestParserNoMatches()
        {
            var src = @"<a href=javascript:void(0)></a>";
            var parser = new L10nParser();
            Assert.AreEqual(0, parser.Parse(src).Count);
        }

        [TestMethod]
        public void TestParserSimpleMatch()
        {
            var src = @"<a href=javascript:void(0)>_s(""Hej"")</a>";
            var parser = new L10nParser();
            var result = parser.Parse(src);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Hej", result[0].Phrase);
        }

        [TestMethod]
        public void TestParserWithLinebreak()
        {
            var src = @"<a href=javascript:void(0)>_s(
                         ""Hej""
                      )</a>";
            var parser = new L10nParser();
            var result = parser.Parse(src);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Hej", result[0].Phrase.Trim());
        }

        [TestMethod]
        public void TestParserWithLinebreak2()
        {
            var src = @"<a href=javascript:void(0)>
                      _s(
                         ""Hej""
                      )
                      _s(
                         ""Nej""
                      )</a>";
            var parser = new L10nParser();
            var result = parser.Parse(src);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Hej", result[0].Phrase.Trim());
            Assert.AreEqual("Nej", result[0].Phrase.Trim());
        }
    }
}
