using System.IO;
using System.Collections.Generic;
using Jil;

namespace MN.L10n.FileProviders
{
	public class FileDataProvider : IL10nDataProvider
	{
		private Options SerializerOptions = Options.ISO8601PrettyPrint;

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
			var tp = new NGettext.Plural.Ast.AstTokenParser();

			var langPath = Path.Combine(FilePath, LanguagesFile);
			List<string> languages = new List<string> { "default" };

			if (File.Exists(langPath))
			{
				var contents = File.ReadAllText(langPath);
				languages = JSON.Deserialize<List<string>>(contents);
			}

			var phrasePath = Path.Combine(FilePath, PhraseFile);
			L10n l10n;
			if (File.Exists(phrasePath))
			{
				var l10nFileContents = File.ReadAllText(phrasePath);
				l10n = JSON.Deserialize<L10n>(l10nFileContents, SerializerOptions);
				l10n.Languages = languages;
			}
			else
			{
				l10n = new L10n
				{
					Languages = languages
				};
				File.WriteAllText(phrasePath, JSON.Serialize(l10n, SerializerOptions));
			}

			foreach (var lang in l10n.Languages)
			{
				var langFileName = Path.Combine(FilePath, string.Format(LanguageFile, lang));
				if (File.Exists(langFileName))
				{
					var phraseFileContents = File.ReadAllText(langFileName);
					var langPhrases = JSON.Deserialize<L10nLanguage>(phraseFileContents, SerializerOptions);
					l10n.LanguagePhrases.Add(lang, langPhrases);
				}
				else
				{
					var nLang = new L10nLanguage
					{
						LanguageName = lang,
						Locale = lang,
						Phrases = new Dictionary<string, L10nPhraseObject>(),
						PluralizationRules = new List<string> { "0", "1" },
						PluralRule = "n != 1"
					};
					File.WriteAllText(langFileName, JSON.Serialize(nLang, SerializerOptions));
					l10n.LanguagePhrases.Add(lang, nLang);
				}

				if (l10n.LanguagePhrases[lang].AstPluralRule == null && !string.IsNullOrWhiteSpace(l10n.LanguagePhrases[lang].PluralRule))
				{
					l10n.LanguagePhrases[lang].AstPluralRule = new NGettext.Plural.AstPluralRule(l10n.LanguagePhrases[lang].PluralizationRules.Count, tp.Parse(l10n.LanguagePhrases[lang].PluralRule));
				}
			}

			return l10n;
		}

		public bool SaveL10n(L10n l10n)
		{
			var l10nFileContents = JSON.Serialize(l10n, SerializerOptions);
			File.WriteAllText(Path.Combine(FilePath, PhraseFile), l10nFileContents);
			return true;
		}
	}
}
