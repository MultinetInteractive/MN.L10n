using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MN.L10n.PhraseMetadata;
using System.Reflection;
using System.Text;
using System.Xml;

namespace MN.L10n
{
    public partial class L10n
    {
        private IL10nDataProvider DataProvider { get; set; }
        private TransactionLanguageProvider LanguageProvider { get; set; }
        public static event EventHandler TranslationsReloaded;

        internal static L10n Instance;

        public static L10n CreateInstance(IL10nLanguageProvider langProvider, IL10nDataProvider dataProvider, Func<IDictionary<object, object>> getScopeContainer = null)
        {
            var l10n = dataProvider.LoadL10n();
            l10n.DataProvider = dataProvider;
            l10n.LanguageProvider = new TransactionLanguageProvider(langProvider, getScopeContainer);
            Instance = l10n;
            return l10n;
        }

        public static IDisposable CreateLanguageScope(string lang)
        {
            return GetInstance().LanguageProvider.LocalLanguageContext(lang);
        }

        [JsonIgnore]
        public List<L10nLanguageItem> Languages { get; set; } = new List<L10nLanguageItem>();
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

        public static L10n GetInstance()
        {
            EnsureInitialized();
            return Instance;
        }

        internal static SemaphoreSlim reloadSemaphore = new SemaphoreSlim(1, 1);

        public static async Task<bool> ReloadFromDataProviderSources(bool removeAllPhrases, CancellationToken token)
        {
            await reloadSemaphore.WaitAsync();
            try
            {
                var prov = GetDataProvider();
                var instance = GetInstance();
                var success = await prov.LoadTranslationFromSources(instance, removeAllPhrases, token);
                if (success)
                {
#pragma warning disable S4220 // Events should have proper arguments
                    TranslationsReloaded?.Invoke(instance, EventArgs.Empty);
#pragma warning restore S4220 // Events should have proper arguments
                }

                return success;
            }
            finally
            {
                reloadSemaphore.Release();
            }
        }

        public static void RemoveAllTranslationReloadedListeners()
        {
            TranslationsReloaded = null;
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

        public static L10nTranslatedString _s(string phrase, object args = null)
        {
            EnsureInitialized();
            return Instance.__getPhrase(phrase, args);
        }

        public static L10nTranslatedString _m(string phrase, object args = null)
        {
            EnsureInitialized();

            return new L10nTranslatedString(Instance.ConvertFromMarkdown(Instance.__getPhrase(phrase, args)));
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

        public static IL10nDataProvider GetDataProvider()
        {
            EnsureInitialized();
            return Instance.DataProvider;
        }

        internal L10nTranslatedString __getPhrase(string phrase, object args = null)
        {
            var cleanedPhrase = phrase.Replace("\r", "");

            if (!Phrases.ContainsKey(cleanedPhrase))
            {
                Phrases.TryAdd(cleanedPhrase, new L10nPhrase());
            }

            var selectedLang = LanguageProvider.GetLanguage();
            var isPluralized = IsPluralized(args);

            if (LanguagePhrases.TryGetValue(selectedLang, out var lang))
            {
                if (lang.Phrases.TryGetValue(cleanedPhrase, out var phr))
                {
                    string getVal;

                    if (phr.r.TryGetValue("0", out getVal))
                    {
                        cleanedPhrase = getVal;
                    }

                    if (isPluralized && lang.AstPluralRule != null)
                    {
                        // Here there be dragons
                        // Dynamic evaluation to get the phrase to use, based on the pluralization rule specified
                        var phraseIndex = lang.AstPluralRule.Evaluate(GetCount(args)).ToString();
                        if (phr.r.TryGetValue(phraseIndex, out getVal))
                        {
                            cleanedPhrase = getVal;
                        }
                    }
                }
                else
                {
                    if (!isPluralized)
                    {
                        LanguagePhrases[selectedLang].Phrases.TryAdd(phrase, new L10nPhraseObject
                        {
                            r = new Dictionary<string, string> { { "0", cleanedPhrase } }
                        });
                    }
                    else
                    {
                        var rules = LanguagePhrases[selectedLang].PluralizationRules;
                        var lpo = new L10nPhraseObject();
                        foreach (var ru in rules)
                        {
                            lpo.r.Add(ru, cleanedPhrase);
                        }
                        LanguagePhrases[selectedLang].Phrases.TryAdd(cleanedPhrase, lpo);
                    }
                }
            }

            var withoutMetadata = PhraseMetadataReplacer.ReplaceMetadata(cleanedPhrase);
            return FormatNamed(withoutMetadata, args);
        }

        private static ConcurrentDictionary<Type, PropertyInfo> IsPluralizedCache = new ConcurrentDictionary<Type, PropertyInfo>();

        public static bool IsPluralized(object args = null)
        {
            if (args == null) return false;

            var t = args.GetType();
            return IsPluralizedCache.GetOrAdd(t, t.GetProperty("__count")) is not null;
        }

        public static long GetCount(object args = null)
        {
            IsPluralizedCache.TryGetValue(args.GetType(), out var p);
            
            if(p is not null)
            {
                return Convert.ToInt64(p.GetValue(args));
                
            }

            return 0;
        }

        private static ConcurrentDictionary<Type, PropertyInfo[]> propCache = new ConcurrentDictionary<Type, PropertyInfo[]>();

        public static L10nTranslatedString FormatNamed(string formatString, object args = null)
        {
            if (args == null) return new L10nTranslatedString(formatString);

            var t = args.GetType();
            var tmpVal = formatString;

            var props = propCache.GetOrAdd(t, tp => tp.GetProperties());

            foreach (var p in props)
            {
                tmpVal = tmpVal.Replace("$" + p.Name + "$", p.GetValue(args)?.ToString());
            }

            return new L10nTranslatedString(tmpVal);
        }
    }
}
