using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Xunit;

namespace MN.L10n.Tests
{
    public class L10nTranslatedStringTests
    {
        [Fact]
        public void ConvertsToJsonString()
        {
            var translatedString = new L10nTranslatedString("rawr");
            Assert.Equal(JsonConvert.SerializeObject("rawr"), JsonConvert.SerializeObject(translatedString));
        }

        [Fact]
        public void ToStringCollectionSimple()
        {
            var list = new List<L10nTranslatedString>()
            {
                new L10nTranslatedString("Test1"),
                new L10nTranslatedString("Test2")
            };
            var res = list.ToStringCollection().ToList();
            Assert.Collection(res, s => Assert.Equal("Test1", s), s => Assert.Equal("Test2", s));
        }

        [Fact]
        public void ToStringCollectionNull()
        {
            List<L10nTranslatedString> list = null;
            var res = list.ToStringCollection()?.ToList();
            Assert.Null(res);
        }
        
        [Fact]
        public void ToStringArraySimple()
        {
            var list = new List<L10nTranslatedString>()
            {
                new L10nTranslatedString("Test1"),
                new L10nTranslatedString("Test2")
            };
            var res = list.ToStringArray().ToList();
            Assert.Collection(res, s => Assert.Equal("Test1", s), s => Assert.Equal("Test2", s));
        }

        [Fact]
        public void ToStringArrayNull()
        {
            List<L10nTranslatedString> list = null;
            var res = list.ToStringArray();
            Assert.Null(res);
        }
        
        [Fact]
        public void ToStringListSimple()
        {
            var list = new List<L10nTranslatedString>()
            {
                new L10nTranslatedString("Test1"),
                new L10nTranslatedString("Test2")
            };
            var res = list.ToStringList().ToList();
            Assert.Collection(res, s => Assert.Equal("Test1", s), s => Assert.Equal("Test2", s));
        }

        [Fact]
        public void ToStringListNull()
        {
            List<L10nTranslatedString> list = null;
            var res = list.ToStringList();
            Assert.Null(res);
        }
    }
}
