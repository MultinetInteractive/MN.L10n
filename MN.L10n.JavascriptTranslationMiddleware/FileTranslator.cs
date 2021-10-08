using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MN.L10n.JavascriptTranslationMiddleware
{
    public class FileTranslator
    {
        private readonly string _languageId;
        private readonly IJavascriptTranslationL10nLanguageProvider _languageProvider;

        public FileTranslator(IJavascriptTranslationL10nLanguageProvider languageProvider, string languageId)
        {
            _languageId = languageId;
            _languageProvider = languageProvider;
        }

        public async Task<string> TranslateFileContentsAsync(FileHandle fileHandle, bool enableCache)
        {
            //We don't want to read the file contents from disk at all if the translation is already cached 
            if (enableCache && _translatedFileContents.TryGetValue(fileHandle.FileName, out var cachedTranslatedContents))
            {
                return cachedTranslatedContents;
            }

            var contents = await fileHandle.GetFileContentsAsync();
            return TranslateFileContents(fileHandle.FileName, contents, enableCache);
        }

        public string TranslateFileContents(string fileName, string contents)
        {
            if (!enableCache)
            {
                return Translate(contents);
            }
            
            return _translatedFileContents.GetOrAdd(fileName, _ => Translate(contents));
        }
        
        private string Translate(string contents)
        {
            if (!_languageProvider.TryGetLanguage(_languageId, out var language)) return contents;

            var parser = new L10nParser();
            List<L10nParser.PhraseInvocation>? invocations = parser.Parse(contents, true);
            Dictionary<int, L10nParser.PhraseInvocation>? starts = invocations.ToDictionary(i => i.StartChar);

            var pluralPhrases = new Dictionary<string, L10nPhraseObject>();

            StringBuilder translationBuilder = new();
            for (var i = 0; i < contents.Length; i++)
            {
                if (starts.TryGetValue(i + 1, out var invocation) &&
                    language.Phrases.TryGetValue(invocation.Phrase, out var languagePhrase))
                {
                    if (languagePhrase.r.Count > 1)
                    {
                        // Om översättningen innehåller pluralizering behöver vi ha kvar originalet och översätta runtime
                        pluralPhrases[invocation.Phrase] = languagePhrase;
                    }
                    else
                    {
                        var translation = languagePhrase.r.First().Value;
                        translationBuilder.Append(translation);

                        if (invocation.IsEscaped) translationBuilder.Append('\\');

                        i = invocation.EndChar - 1;

                        continue;
                    }
                }

                translationBuilder.Append(contents[i]);
            }

            var translatedCode = translationBuilder.ToString();
            if (pluralPhrases.Count == 0) return translatedCode;

            StringBuilder addPhrasesCode = new("var x = l10n.Phrases;");
            foreach (var (key, value) in pluralPhrases)
                addPhrasesCode.Append($"x[\"{key}\"] = {JsonConvert.SerializeObject(value)};");

            return $@"(function(){{ {addPhrasesCode} }})();{Environment.NewLine}{translatedCode}";
        }
    }
}
