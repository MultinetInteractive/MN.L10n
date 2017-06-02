using Jil;
using System;
using System.Collections.Generic;

namespace MN.L10n
{
	public partial class L10n
	{
		private IL10nDataProvider DataProvider { get; set; }
		private IL10nLanguageProvider LanguageProvider { get; set; }

		internal static L10n Instance;

		public static L10n CreateInstance(IL10nLanguageProvider langProvider, IL10nDataProvider dataProvider)
		{
			var l10n = dataProvider.LoadL10n();
			l10n.DataProvider = dataProvider;
			l10n.LanguageProvider = langProvider;
			Instance = l10n;
			return l10n;
		}
		public List<string> Languages { get; set; } = new List<string>();
		public Dictionary<string, L10nPhrase> Phrases { get; set; } = new Dictionary<string, L10nPhrase>();

		[JilDirective(Ignore = true)]
		public Dictionary<string, L10nLanguage> LanguagePhrases { get; set; } = new Dictionary<string, L10nLanguage>();

		public static bool SaveDataProvider()
		{
			if (Instance == null) throw new Exception("You must use L10n.CreateInstance(langProvider, dataProvider) to create an instance before using this.");
			return Instance.DataProvider.SaveL10n(Instance);
		}

		public static string _s(string phrase, object args = null)
		{
			if (Instance == null) throw new Exception("You must use L10n.CreateInstance(langProvider, dataProvider) to create an instance before using this.");
			return Instance.__getPhrase(phrase, args);
		}

		public static string _m(string phrase, object args = null)
		{
			if (Instance == null) throw new Exception("You must use L10n.CreateInstance(langProvider, dataProvider) to create an instance before using this.");

			return Instance.ConvertFromMarkdown(Instance.__getPhrase(phrase, args));
		}
		
		public string ConvertFromMarkdown(string phrase)
		{
			return CommonMark.CommonMarkConverter.Convert(phrase);
		}

		public static string GetLanguage()
		{
			if (Instance == null) throw new Exception("You must use L10n.CreateInstance(langProvider, dataProvider) to create an instance before using this.");
			return Instance.LanguageProvider.GetLanguage();
		}

		internal string __getPhrase(string phrase, object args = null)
		{
			if (!Phrases.ContainsKey(phrase))
			{
				Phrases.Add(phrase, new L10nPhrase());
			}

			var selectedLang = LanguageProvider.GetLanguage();
			var isPluralized = IsPluralized(args);

			if (LanguagePhrases.ContainsKey(selectedLang))
			{
				if (LanguagePhrases[selectedLang].Phrases.ContainsKey(phrase))
				{
					var phr = LanguagePhrases[selectedLang].Phrases[phrase];

					if (phr.r.ContainsKey("x"))
					{
						phrase = phr.r["x"];
					}

					if (isPluralized)
					{
						foreach (var rule in phr.r)
						{
							if (rule.Key == GetCount(args).ToString())
							{
								phrase = rule.Value;
							}
						}
					}
				}
				else
				{
					if (!isPluralized)
					{
						LanguagePhrases[selectedLang].Phrases.Add(phrase, new L10nPhraseObject
						{
							r = new Dictionary<string, string> { { "x", phrase } }
						});
					}
					else
					{
						var rules = LanguagePhrases[selectedLang].PluralizationRules;
						var lpo = new L10nPhraseObject();
						foreach (var ru in rules)
						{
							lpo.r.Add(ru, phrase);
						}
						LanguagePhrases[selectedLang].Phrases.Add(phrase, lpo);
					}
				}
			}

			return FormatNamed(phrase, args);
		}

		public static bool IsPluralized(object args = null)
		{
			if (args == null) return false;
			var t = args.GetType();
			foreach (var p in t.GetProperties())
			{
				if (p.Name == "__count") return true;
			}

			return false;
		}

		public static int GetCount(object args = null)
		{
			if (args == null) return 0;
			var t = args.GetType();
			foreach (var p in t.GetProperties())
			{
				if (p.Name == "__count") return (int)p.GetValue(args);
			}

			return 0;
		}

		public static string FormatNamed(string formatString, object args = null)
		{
			if (args == null) return formatString;

			var t = args.GetType();
			var tmpVal = formatString;
			foreach (var p in t.GetProperties())
			{
				tmpVal = tmpVal.Replace("$" + p.Name + "$", p.GetValue(args).ToString());
			}

			return tmpVal;
		}
	}
}
