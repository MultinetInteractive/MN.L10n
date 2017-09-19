using System;
using System.Collections.Generic;

namespace MN.L10n
{
	public class PhrasesRewriter
	{
		internal L10n _phrases;
		internal Dictionary<string, Dictionary<object, L10nPhraseObject>> _phraseDic = new Dictionary<string, Dictionary<object, L10nPhraseObject>>();
		internal string LanguageIdentifier;

		internal List<string> unusedPhrases = new List<string>();
		public PhrasesRewriter(string className, string languageIdentifier, L10n phraseRepo, params string[] methods) : base()
		{
			if (!string.IsNullOrWhiteSpace(className))
				phraseClassName = className.Trim();
			LanguageIdentifier = languageIdentifier.Trim();
			_phrases = phraseRepo;
			
			var allPhrases = _phrases.Phrases.Keys;
			foreach (var p in allPhrases)
			{
				_phraseDic.Add(p, new Dictionary<object, L10nPhraseObject>());
				unusedPhrases.Add(p);
				_phrases.Phrases[p].Usages = 0;
			}

			foreach (var fLang in _phrases.LanguagePhrases)
			{
				var langKey = fLang.Key;
				var translatedPhrases = fLang.Value.Phrases;
				foreach (var trpr in translatedPhrases)
				{
					if (!_phraseDic.ContainsKey(trpr.Key))
					{
						_phraseDic.Add(trpr.Key, new Dictionary<object, L10nPhraseObject>());
					}
					_phraseDic[trpr.Key].Add(langKey, trpr.Value);
				}
			}

			if (methods.Length > 0)
				PhraseMethods = methods;
		}

		public bool SavePhrasesToFile()
		{
			foreach (var unused in unusedPhrases)
			{
				_phrases.Phrases.Remove(unused);
			}

			return L10n.SaveDataProvider();
		}

		internal string phraseClassName = "Phrase" + Guid.NewGuid().ToString().Replace("-", "");
		internal string[] PhraseMethods = new string[] { "_s", "_m" };
	}
}
