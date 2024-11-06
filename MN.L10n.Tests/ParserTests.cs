using System.IO;
using System.Linq;
using Xunit;

namespace MN.L10n.Tests
{
    public class ParserTests
    {
        [Fact]
        public void TestParserNoMatches()
        {
            var src = @"<a href=javascript:void(0)></a>";
            var parser = new L10nParser();
            Assert.Empty(parser.Parse(src));
        }

        [Fact]
        public void TestParserSimpleMatch()
        {
            var src = @"<a href=javascript:void(0)>_s(""Hej"")</a>";
            var parser = new L10nParser();
            var result = parser.Parse(src).ToList();
            Assert.Single(result);
            Assert.Equal("Hej", result[0].Phrase);
        }

        [Fact]
        public void TestParserWithLinebreak()
        {
            var src = @"<a href=javascript:void(0)>_s(
                         ""Hej""
                      )</a>";
            var parser = new L10nParser();
            var result = parser.Parse(src).ToList();
            Assert.Single(result);
            Assert.Equal("Hej", result[0].Phrase.Trim());
        }

        [Fact]
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
            var result = parser.Parse(src).ToList();
            Assert.Equal(2, result.Count);
            Assert.Equal("Hej", result[0].Phrase.Trim());
            Assert.Equal("Nej", result[1].Phrase.Trim());
        }

        [Fact]
        public void TestParserWithVerbatimLinebreak()
        {
            var src = @"<a href=javascript:void(0)>
                      _s(
                         @""Hej
Nej""
                      )
                      </a>";
            var parser = new L10nParser();
            var result = parser.Parse(src).ToList();
            Assert.Single(result);
            Assert.Equal(@"Hej\nNej", result[0].Phrase.Trim());
        }

        [Fact]
        public void TestDoesNotGoOutOfBounds()
        {
            var src = @"<a href=javascript:void(0)>
                      _s(";
            var parser = new L10nParser();
            var result = parser.Parse(src);
            Assert.Empty(result);
        }

        [Fact]
        public void TestWorksWithMultiLineText()
        {
            var src = @"<text>
                        @_sr(@""Snart är det dags att välja mellan Basic och Premium!<br />
Er testperiod av Premium löper ut $expirationDate$.<br />
Kontakta $ownerName$ och be om uppgradering till Premium redan idag.<br />
Ni kan också kontakta oss på <a href=""""https://support.semesterlistan.se"""" target=""""_blank"""">supporten</a>. Så hjälper vi till!"", new
                        {
                            expirationDate = Legacy.GetTrialExpiration(user.CompanyId).ToShortDateString(),
                            ownerName = Legacy.GetOwnerString(user.CompanyId)
                        })
                    </text>";

            var parser = new L10nParser();
            var result = parser.Parse(src);
            Assert.Single(result);
        }

        [Fact]
        public void TestWorksWithLiteralTemplateStrings()
        {
            var src = @"function javascriptMethod() { return _s(`This text will also be found by the parser!`); }";

            var parser = new L10nParser();
            var result = parser.Parse(src);
            Assert.Single(result);
        }
        
        [Fact]
        public void FulTest()
        {
            var src = File.ReadAllText(
                "D:\\git\\dm\\avtalshantering.app\\Scripts\\Webpack\\React\\Pages\\Settings\\Resources\\Tabs\\EditCompanyNotificationSettings.tsx");
            
            var parser = new L10nParser();
            var result = parser.Parse(src).ToArray();
            
            Assert.Equal("Följande inställningar gäller som standard för nya användare.\nAnvändare kan välja vilka notifikationer de vill få under \"Mina inställningar\".", result[2].Phrase);
        }

        [Fact]
        public void LineBreakCharInCall()
        {
            var parser = new L10nParser();
            var result = parser.Parse("_s('Hello\\nBrother')");

            Assert.Collection(result, x => Assert.Equal("Hello\nBrother", x.Phrase));
        }
    }
}
