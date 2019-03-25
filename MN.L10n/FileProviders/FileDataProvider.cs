using System.Collections.Concurrent;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace MN.L10n.FileProviders
{
    public class FileDataProvider : IL10nDataProvider
    {
        private JsonSerializerSettings SerializerOptions = new JsonSerializerSettings
        {
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
            L10n l10n = LoadLanguages();
            LoadTranslations(l10n);

            return l10n;
        }

        public L10n LoadLanguages()
        {
            var langPath = Path.Combine(FilePath, LanguagesFile);
            List<L10nLanguageItem> languages = new List<L10nLanguageItem> { new L10nLanguageItem { LanguageId = "default" } };

            if (File.Exists(langPath))
            {
                var contents = File.ReadAllText(langPath);
                try
                {
                    languages = JsonConvert.DeserializeObject<List<L10nLanguageItem>>(contents);
                }
                catch
                {
                    var _localLanguages = JsonConvert.DeserializeObject<List<string>>(contents);
                    languages.Clear();
                    foreach (var locLang in _localLanguages)
                    {
                        languages.Add(new L10nLanguageItem { LanguageId = locLang });
                    }

                    File.WriteAllText(langPath, JsonConvert.SerializeObject(languages, SerializerOptions));
                }
            }

            var phrasePath = Path.Combine(FilePath, PhraseFile);

            if (File.Exists(phrasePath))
            {
                var l10nFileContents = File.ReadAllText(phrasePath);
                var l10n = JsonConvert.DeserializeObject<L10n>(l10nFileContents, SerializerOptions);
                l10n.Languages = languages;
                return l10n;
            }
            else
            {
                var l10n = new L10n
                {
                    Languages = languages
                };
                File.WriteAllText(phrasePath, JsonConvert.SerializeObject(l10n, SerializerOptions));

                return l10n;
            }
        }


        public void LoadTranslations(L10n l10n)
        {
            var tp = new NGettext.Plural.Ast.AstTokenParser();
            foreach (var lang in l10n.Languages.Select(l => l.LanguageId))
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


        public bool SaveL10n(L10n l10n)
        {
            var l10nFileContents = JsonConvert.SerializeObject(l10n, SerializerOptions);
            File.WriteAllText(Path.Combine(FilePath, PhraseFile), l10nFileContents);
            return true;
        }

        public async Task LoadTranslationFromSources(L10n l10n, CancellationToken token)
        {
            bool errorLoadingSources = false;
            Exception _ex = null;
            // If we don't have anything to fetch from any sources, don't bother
            if (!l10n.Languages.Any(l => l.Sources.Any())) return;

            using (var cli = new HttpClient())
            {
                cli.DefaultRequestHeaders.ConnectionClose = true;

                foreach (var lang in l10n.Languages)
                {
                    var l10nLang = l10n.LanguagePhrases[lang.LanguageId];
                    var sources = lang.Sources.ToList();
                    // Read the sources in reverse order, "original" translations should be first in the list
                    sources.Reverse();
                    foreach (var source in sources)
                    {
                        if (source.StartsWith("http") && Uri.IsWellFormedUriString(source, UriKind.Absolute))
                        {
                            string translationSource = string.Empty;
                            try { await cli.GetStringAsync(source); } catch(Exception ex) { errorLoadingSources = true; _ex = ex; }

                            if(errorLoadingSources)
                            {
                                Console.WriteLine("error l10n: Could not load translation from {0}", source);
                                if (_ex != null)
                                {
                                    Console.WriteLine(_ex.ToString());
                                }

                            }

                            if (!string.IsNullOrWhiteSpace(translationSource))
                            {
                                try
                                {
                                    var translationPhrases = JsonConvert.DeserializeObject<L10nLanguage>(translationSource, SerializerOptions);
                                    if (translationPhrases.Phrases.Count > 0)
                                    { 
                                        foreach(var phrase in translationPhrases.Phrases)
                                        {
                                            l10nLang.Phrases[phrase.Key] = phrase.Value;
                                        }
                                    }
                                }
                                catch(Exception ex)
                                {
                                    Console.WriteLine("error l10n: Cannot read translation from {0}", source);
                                    Console.WriteLine(ex.ToString());
                                }
                            }
                        }

                        if (token.IsCancellationRequested)
                        {
                            throw new TaskCanceledException();
                        }
                    }

                    var langFileName = Path.Combine(FilePath, string.Format(LanguageFile, lang.LanguageId));
                    File.WriteAllText(langFileName, JsonConvert.SerializeObject(l10nLang, SerializerOptions));

                    if (token.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }
                }
                cli.Dispose();
            }

            SaveL10n(l10n);
        }
    }
}
