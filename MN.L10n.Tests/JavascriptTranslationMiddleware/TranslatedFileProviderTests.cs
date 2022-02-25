using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using MN.L10n.JavascriptTranslationMiddleware;
using Xunit;

namespace MN.L10n.Tests.JavascriptTranslationMiddleware
{
    public class TranslatedFileProviderTests
    {
        [Fact]
        public void TranslatesSimpleFile()
        {
            var (fakes, translator) = CreateFakes();
            fakes.AddPhrase("Dra och släpp de kategorierna du vill lägga till i trädet till vänster", "Do the thing");
            
            var translation = translator.TranslateFileContents("console.log(_s(\"Dra och släpp de kategorierna du vill lägga till i trädet till vänster\"));");
            
            Assert.Equal($"console.log(_s(\"Do the thing\"));", translation);
        }

        [Fact]
        public void TranslatesEvaledCall()
        {
            var (fakes, translator) = CreateFakes();
            fakes.AddPhrase("Nej", "No");
            
            var translation = translator.TranslateFileContents("console.log(eval(\"_s(\\\"Nej\\\")\";");
            
            Assert.Equal($"console.log(eval(\"_s(\\\"No\\\")\";", translation);
        }

        [Fact]
        public void HandlesPluralizedPhrase()
        {
            const string fileName = "testfile";
            var (fakes, translator) = CreateFakes();
            const string phrase = "$__count$ minuter sedan";
            fakes.AddPhrase(phrase, "Now", "$__count$ minutes ago");
            
            var translation = translator.TranslateFileContents("_s(\"$__count$ minuter sedan\", {__count: 7});");
            
            Assert.Equal($"(function(){{ var x = l10n.Phrases;x[\"{phrase}\"] = {{\"r\":{{\"0\":\"Now\",\"1\":\"$__count$ minutes ago\"}}}}; }})();\r\n_s(\"$__count$ minuter sedan\", {{__count: 7}});", translation);
        }
        
        [Fact]
        public void HandlesPluralizedPhraseInEval()
        {
            const string fileName = "testfile";
            var (fakes, translator) = CreateFakes();
            const string phrase = "$__count$ minuter sedan";
            fakes.AddPhrase(phrase, "Now", "$__count$ minutes ago");
            
            var translation = translator.TranslateFileContents("eval(\"_s(\\\"$__count$ minuter sedan\\\", {__count: 7});\");");
            
            Assert.Equal($"(function(){{ var x = l10n.Phrases;x[\"{phrase}\"] = {{\"r\":{{\"0\":\"Now\",\"1\":\"$__count$ minutes ago\"}}}}; }})();\r\neval(\"_s(\\\"$__count$ minuter sedan\\\", {{__count: 7}});\");", translation);
        }

        [Fact]
        public void HandlesTranslationWithQuotes()
        {
            var (fakes, translator) = CreateFakes();
            fakes.AddPhrase("Dra och släpp de kategorierna du vill lägga till i trädet till vänster", "Do \"the\" thing");
            
            var translation = translator.TranslateFileContents("_s(\"Dra och släpp de kategorierna du vill lägga till i trädet till vänster\");");
            
            Assert.Equal("_s(\"Do \\\"the\\\" thing\");", translation);
        }
        
        [Fact]
        public void HandlesTranslationWithQuotes2()
        {
            var (fakes, translator) = CreateFakes();
            fakes.AddPhrase("Dra och släpp de kategorierna du vill lägga till i trädet till vänster", "Do \"the\" thing");
            
            var translation = translator.TranslateFileContents("_s('Dra och släpp de kategorierna du vill lägga till i trädet till vänster');");
            
            Assert.Equal("_s('Do \"the\" thing');", translation);
        }
        
        [Fact]
        public void HandlesTranslationWithQuotes3()
        {
            var (fakes, translator) = CreateFakes();
            fakes.AddPhrase("Dra och släpp de kategorierna du vill lägga till i trädet till vänster", "Do 'the' thing");
            
            var translation = translator.TranslateFileContents("_s('Dra och släpp de kategorierna du vill lägga till i trädet till vänster');");
            
            Assert.Equal("_s('Do \\\'the\\\' thing');", translation);
        }
        
        private (Fakes fakes, FileTranslator translator) CreateFakes(string languageId = "2")
        {
            var language = new L10nLanguage
            {
                LanguageName = languageId,
                PluralizationRules = new List<string>(),
                PluralRule = "0",
                Locale = "en-GB",
                Phrases = new ConcurrentDictionary<string, L10nPhraseObject>()
            };
            
            var fakes = new Fakes(language);
            var translator = new FileTranslator(fakes.LanguageProvider, fakes.FileHandle, languageId, fakes.Logger);

            L10nLanguage tmp;
            A.CallTo(() => fakes.LanguageProvider.TryGetLanguage(languageId, out tmp))
                .Returns(true)
                .AssignsOutAndRefParameters(language);
            
            return (fakes, translator);
        }
        
        private class Fakes
        {
            private readonly L10nLanguage _language;
            public IJavascriptTranslationL10nLanguageProvider LanguageProvider =
                A.Fake<IJavascriptTranslationL10nLanguageProvider>();
            public readonly IFileHandle FileHandle = A.Fake<IFileHandle>();
            public readonly ILogger<FileTranslator> Logger = A.Fake<ILogger<FileTranslator>>();

            public Fakes(L10nLanguage language)
            {
                _language = language;
            }

            public void AddPhrase(string phrase, string translation)
            {
                AddPhrase(phrase, new[]{ translation });
            }
            
            public void AddPhrase(string phrase, params string[] translations)
            {
                if (translations.Length == 0)
                {
                    throw new Exception("At least one translation must be provided");
                }
                
                var phraseObj = new L10nPhraseObject();
                _language.Phrases[phrase] = phraseObj;
                for (var i = 0; i < translations.Length; i++)
                {
                    phraseObj.r[$"{i}"] = translations[i];
                }
            }
        }
    }
}
