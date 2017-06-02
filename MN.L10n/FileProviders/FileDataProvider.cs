using System.IO;
using System.Collections.Generic;
using Jil;

namespace MN.L10n.FileProviders
{
	public class FileDataProvider : IL10nDataProvider
	{
		private Options SerializerOptions = Options.ISO8601;

		private string FilePath { get; set; }
		private string PhraseFile { get; set; }
		private string LanguageFile { get; set; }
		public FileDataProvider(string path, string l10nFileName = "phrases.json", string l10nPhraseFileNameFormat = "language-{0}.json")
		{
			FilePath = path;
			PhraseFile = l10nFileName;
			LanguageFile = l10nPhraseFileNameFormat;
		}
		public L10n LoadL10n()
		{
			var phrasePath = Path.Combine(FilePath, PhraseFile);
			L10n l10n;
			if (File.Exists(phrasePath))
			{
				var l10nFileContents = File.ReadAllText(phrasePath);
				l10n = JSON.Deserialize<L10n>(l10nFileContents, SerializerOptions);
			}
			else
			{
				l10n = new L10n
				{
					Languages = new List<string> { "default" }
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
						PluralizationRules = new List<string> { "x" }
					};
					File.WriteAllText(langFileName, JSON.Serialize(nLang, SerializerOptions));
					l10n.LanguagePhrases.Add(lang, nLang);
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
