﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MN.L10n.FileProviders
{
	public class FileDataProvider : IL10nDataProvider
	{
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
			var l10nFileContents = File.ReadAllText(Path.Combine(FilePath, PhraseFile));
			var l10n = Newtonsoft.Json.JsonConvert.DeserializeObject<L10n>(l10nFileContents);

			foreach (var lang in l10n.Languages)
			{
				var phraseFileContents = File.ReadAllText(Path.Combine(FilePath, string.Format(LanguageFile, lang)));
				var langPhrases = Newtonsoft.Json.JsonConvert.DeserializeObject<L10nLanguage>(phraseFileContents);
				l10n.LanguagePhrases.Add(lang, langPhrases);
			}

			return l10n;
		}

		public bool SaveL10n(L10n l10n)
		{
			var l10nFileContents = Newtonsoft.Json.JsonConvert.SerializeObject(l10n);
			File.WriteAllText(Path.Combine(FilePath, PhraseFile), l10nFileContents);
			return true;
		}
	}
}