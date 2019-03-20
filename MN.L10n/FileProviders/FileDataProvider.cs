using System.Collections.Concurrent;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MN.L10n.FileProviders
{
	public class FileDataProvider : IL10nDataProvider
	{
		private JsonSerializerSettings SerializerOptions = new JsonSerializerSettings {
			Formatting = Formatting.Indented,
			DateFormatHandling = DateFormatHandling.IsoDateFormat
		};

	private string FilePath { get; set; }
		private string PhraseFile { get; set; }
		private string LanguagesFile { get; set; }
		private string LanguageFile { get; set; }
		public FileDataProvider(string path, string l10nFileName = "phrases.json", string l10nPhraseFileNameFormat = "language-{0}.json", string l10nLanguagesFileName = "languages.json")
		{
			FilePath = path;
			PhraseFile = l10nFileName;
			LanguagesFile = l10nLanguagesFileName;
			LanguageFile = l10nPhraseFileNameFormat;
		}
		public L10n LoadL10n()
        {
            L10n l10n = new L10n();

            LoadLanguages(ref l10n);
            LoadTranslations(ref l10n);

            return l10n;
        }

        public void LoadTranslations(ref L10n l10n)
        {
            var tp = new NGettext.Plural.Ast.AstTokenParser();
            foreach (var lang in l10n.Languages)
            {
                var langFileName = Path.Combine(FilePath, string.Format(LanguageFile, lang));
                if (File.Exists(langFileName))
                {
                    var phraseFileContents = File.ReadAllText(langFileName);
                    var langPhrases = JsonConvert.DeserializeObject<L10nLanguage>(phraseFileContents, SerializerOptions);
                    l10n.LanguagePhrases.TryAdd(lang, langPhrases);
                }
                else
                {
                    var nLang = new L10nLanguage
                    {
                        LanguageName = lang,
                        Locale = lang,
                        Phrases = new ConcurrentDictionary<string, L10nPhraseObject>(),
                        PluralizationRules = new List<string> { "0", "1" },
                        PluralRule = "n != 1"
                    };
                    File.WriteAllText(langFileName, JsonConvert.SerializeObject(nLang, SerializerOptions));
                    l10n.LanguagePhrases.TryAdd(lang, nLang);
                }

                if (l10n.LanguagePhrases[lang].AstPluralRule == null && !string.IsNullOrWhiteSpace(l10n.LanguagePhrases[lang].PluralRule))
                {
                    l10n.LanguagePhrases[lang].AstPluralRule = new NGettext.Plural.AstPluralRule(l10n.LanguagePhrases[lang].PluralizationRules.Count, tp.Parse(l10n.LanguagePhrases[lang].PluralRule));
                }
            }
        }

        public void LoadLanguages(ref L10n l10n)
        {
            var langPath = Path.Combine(FilePath, LanguagesFile);
            List<string> languages = new List<string> { "default" };

            if (File.Exists(langPath))
            {
                var contents = File.ReadAllText(langPath);
                languages = JsonConvert.DeserializeObject<List<string>>(contents);
            }

            var phrasePath = Path.Combine(FilePath, PhraseFile);

            if (File.Exists(phrasePath))
            {
                var l10nFileContents = File.ReadAllText(phrasePath);
                l10n = JsonConvert.DeserializeObject<L10n>(l10nFileContents, SerializerOptions);
                l10n.Languages = languages;
            }
            else
            {
                l10n = new L10n
                {
                    Languages = languages
                };
                File.WriteAllText(phrasePath, JsonConvert.SerializeObject(l10n, SerializerOptions));
            }
        }

        public bool SaveL10n(L10n l10n)
		{
			var l10nFileContents = JsonConvert.SerializeObject(l10n, SerializerOptions);
			File.WriteAllText(Path.Combine(FilePath, PhraseFile), l10nFileContents);
			return true;
		}
	}
}
