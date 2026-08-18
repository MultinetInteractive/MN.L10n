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
            Assert.Equal("Hej\nNej", result[0].Phrase.Trim());
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
        public void LineBreakCharInCall()
        {
            var parser = new L10nParser();
            var result = parser.Parse("_s('Hello\\nBrother')");

            Assert.Collection(result, x => Assert.Equal("Hello\nBrother", x.Phrase));
        }

        [Fact]
        public void TestEscapedStringContainerCharacter()
        {
            var parser = new L10nParser();
            var result = parser.Parse(@"_s(""Hello \""friend\"". How it do?"")");

            Assert.Collection(result, x => Assert.Equal(@"Hello ""friend"". How it do?", x.Phrase));            
        }
        
        [Fact]
        public void TestEscapedStringContainerCharacter2()
        {
            var parser = new L10nParser();
            var result = parser.Parse(@"_s('Hello \""friend\"". How it do?')");

            Assert.Collection(result, x => Assert.Equal(@"Hello \""friend\"". How it do?", x.Phrase));            
        }
        
        [Fact]
        public void TestEscapedStringContainerCharacter3()
        {
            var parser = new L10nParser();
            var result = parser.Parse(@"_s('Hello \'friend\'. How it do?')");

            Assert.Collection(result, x => Assert.Equal(@"Hello 'friend'. How it do?", x.Phrase));            
        }
        
        [Fact]
        public void TestEscapedStringContainerCharacterFirstChar()
        {
            var parser = new L10nParser();
            var result = parser.Parse(@"_s(""\""friend\""!"")");

            Assert.Collection(result, x => Assert.Equal(@"""friend""!", x.Phrase));
        }
        
        [Fact]
        public void TestEscapedStringContainerCharacterVerbatim()
        {
            var src = @"<a href=javascript:void(0)>
                      _s(
                         @""Hej """"bror""""
Nej""
                      )
                      </a>";
            var parser = new L10nParser();
            var result = parser.Parse(src).ToList();
            Assert.Single(result);
            Assert.Equal("Hej \"bror\"\nNej", result[0].Phrase.Trim());
        }
        
        [Fact]
        public void TestNewlineInJsTemplateString()
        {
            var src = "_s(`Hello\r\nbrother!`)";
            var parser = new L10nParser();
            var result = parser.Parse(src).ToList();
            Assert.Single(result);
            Assert.Equal("Hello\nbrother!", result[0].Phrase.Trim());
        }
        
        [Fact]
        public void TestNewLineInVerbatimString()
        {
            var src = @"
var body = _s(@""(Detta är ett automatiserat meddelande)<br/><br/>
<b>Användaruppgifter för $product$</b><br/><br/>
Dessa uppgifter har registrerats i $product$. Du har möjlighet att ändra på uppgifterna genom att logga in.<br/><br/>
Kontouppgifter<br/>
Företagskonto: $companyName$
<br/><br/>Namn: $fullname$
<br/>Användarnamn: $username$
<br/>E-post: $email$
<br/>Adress: $address$
<br/>Ort: $city$
<br/>Postnr: $zip$
<br/>Telefon: $phone$"", 
            new
            {
				companyName = user.CompanyName,
				fullname = mailToUser.Fullname,
				username = mailToUser.Username,
				email = mailToUser.Email,
				address = mailToUser.Address1,
				city = mailToUser.City,
				zip = mailToUser.Zipcode,
				phone = mailToUser.Phone,
				product = productName,
			});
";
            var parser = new L10nParser();
            var result = parser.Parse(src).ToList();
            Assert.Single(result);
            Assert.Equal("(Detta är ett automatiserat meddelande)<br/><br/>\n<b>Användaruppgifter för $product$</b><br/><br/>\nDessa uppgifter har registrerats i $product$. Du har möjlighet att ändra på uppgifterna genom att logga in.<br/><br/>\nKontouppgifter<br/>\nFöretagskonto: $companyName$\n<br/><br/>Namn: $fullname$\n<br/>Användarnamn: $username$\n<br/>E-post: $email$\n<br/>Adress: $address$\n<br/>Ort: $city$\n<br/>Postnr: $zip$\n<br/>Telefon: $phone$", result[0].Phrase.Trim());
        }
    }
}
