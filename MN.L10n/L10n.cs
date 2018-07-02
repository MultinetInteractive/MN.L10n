using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;

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

        [JsonIgnore]
        public List<string> Languages { get; set; } = new List<string>();
        public ConcurrentDictionary<string, L10nPhrase> Phrases { get; set; } = new ConcurrentDictionary<string, L10nPhrase>();

		[JsonIgnore]
		public ConcurrentDictionary<string, L10nLanguage> LanguagePhrases { get; set; } = new ConcurrentDictionary<string, L10nLanguage>();

        public static L10nLanguage GetL10nLanguage(string language)
        {
            EnsureInitialized();
            if (Instance.LanguagePhrases.TryGetValue(language, out var l10nLanguage))
            {
                return l10nLanguage;
            }
            throw new Exception($"Unknown language : {language}");
        }

        public static bool SaveDataProvider()
        {
            EnsureInitialized();
            return Instance.DataProvider.SaveL10n(Instance);
        }

        private static void EnsureInitialized()
        {
            if (Instance == null) throw new Exception("You must use L10n.CreateInstance(langProvider, dataProvider, fileResolver) to create an instance before using this.");
        }

        public static string _s(string phrase, object args = null)
        {
            EnsureInitialized();
            return Instance.__getPhrase(phrase, args);
        }

        public static string _m(string phrase, object args = null)
        {
            EnsureInitialized();

            return Instance.ConvertFromMarkdown(Instance.__getPhrase(phrase, args));
        }

        public string ConvertFromMarkdown(string phrase)
        {
            return CommonMark.CommonMarkConverter.Convert(phrase);
        }

        public static string GetLanguage()
        {
            EnsureInitialized();
            return Instance.LanguageProvider.GetLanguage();
        }

		public static T GetLanguageProvider<T>() where T : IL10nLanguageProvider
		{
			EnsureInitialized();
			return (T)Instance.LanguageProvider;
		}

        internal string __getPhrase(string phrase, object args = null)
        {
            if (!Phrases.ContainsKey(phrase))
            {
                Phrases.TryAdd(phrase, new L10nPhrase());
            }

            var selectedLang = LanguageProvider.GetLanguage();
            var isPluralized = IsPluralized(args);

            if (LanguagePhrases.TryGetValue(selectedLang, out var lang))
            {
                if (lang.Phrases.TryGetValue(phrase, out var phr))
                {
                    if (phr.r.ContainsKey("0"))
                    {
                        phrase = phr.r["0"];
                    }

                    if (isPluralized && lang.AstPluralRule != null)
                    {
                        // Here there be dragons
                        // Dynamic evaluation to get the phrase to use, based on the pluralization rule specified
                        var phraseIndex = lang.AstPluralRule.Evaluate(GetCount(args)).ToString();
                        if (phr.r.ContainsKey(phraseIndex))
                        {
                            phrase = phr.r[phraseIndex];
                        }
                    }
                }
                else
                {
                    if (!isPluralized)
                    {
                        LanguagePhrases[selectedLang].Phrases.TryAdd(phrase, new L10nPhraseObject
                        {
                            r = new Dictionary<string, string> { { "0", phrase } }
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
                        LanguagePhrases[selectedLang].Phrases.TryAdd(phrase, lpo);
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

        public static long GetCount(object args = null)
        {
            if (args == null) return 0;
            var t = args.GetType();
            foreach (var p in t.GetProperties())
            {
                if (p.Name == "__count")
                {
                    long.TryParse(p.GetValue(args).ToString(), out long __count);
                    return __count;
                }
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
                tmpVal = tmpVal.Replace("$" + p.Name + "$", p.GetValue(args)?.ToString());
            }

            return tmpVal;
        }
    }
}
