using Newtonsoft.Json;
using Xunit;

namespace MN.L10n.Tests
{
    public class TranslatedStringJsonSerializerTests
    {
        [Theory]
        [InlineData("testing", "\"testing\"")]
        [InlineData(null, "null")]
        [InlineData("phrase with \"quotes\"", "\"phrase with \\\"quotes\\\"\"")]
        public void SerializesStringsCorrectly(string input, string expected)
        {
            var value = new L10nTranslatedString(input);
            var serialized = JsonConvert.SerializeObject(value);
            Assert.Equal(expected, serialized);
        }

        [Theory]
        [InlineData("\"testing\"", "testing")]
        [InlineData("\"phrase with \\\"quotes\\\"\"", "phrase with \"quotes\"")]
        public void DeserializesCorrectly(string input, string expected)
        {
            var expectedTranslated = new L10nTranslatedString(expected);
            var deserialized = JsonConvert.DeserializeObject<L10nTranslatedString>(input);
            Assert.Equal(expectedTranslated, deserialized);
        }

        [Fact]
        public void DeserializesNullCorrectlyWhenNullable()
        {
            var result = JsonConvert.DeserializeObject<L10nTranslatedString?>("null");
            Assert.Null(result);
        }
        
        [Fact]
        public void DeserializesNullCorrectlyWhenNotNullable()
        {
            var result = JsonConvert.DeserializeObject<L10nTranslatedString>("null");
            var expected = new L10nTranslatedString(null);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("testing")]
        [InlineData("other")]
        public void SerializesAndDeserializesClassCorrectly(string phrase)
        {
            var withTranslatedProp = new TestClassWithTranslatedStringProp(phrase);
            var withStringProp = new TestClassWithStringProp(phrase);

            var expected = JsonConvert.SerializeObject(withStringProp);
            var result = JsonConvert.SerializeObject(withTranslatedProp);
            
            Assert.Equal(expected, result);

            var deserialized = JsonConvert.DeserializeObject<TestClassWithTranslatedStringProp>(expected);
            Assert.Equal(withStringProp.Original, deserialized.Original);
            Assert.Equal(withStringProp.Translated, deserialized.Translated.ToString());
        }
        
        private class TestClassWithTranslatedStringProp
        {
            public L10nTranslatedString Translated { get; set; }
            public string Original { get; set; }

            public TestClassWithTranslatedStringProp(string phrase)
            {
                Translated = new L10nTranslatedString(phrase + "_translated");
                Original = phrase;
            }
        }
        
        private class TestClassWithStringProp
        {
            public string Translated { get; set; }
            public string Original { get; set; }

            public TestClassWithStringProp(string phrase)
            {
                Translated = phrase + "_translated";
                Original = phrase;
            }
        }
    }
}
