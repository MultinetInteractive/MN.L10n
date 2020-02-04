using NGettext.Plural;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MN.L10n
{
    public class L10nLanguageItem
    {
        public string LanguageId { get; set; }
        public List<string> Sources { get; set; } = new List<string>();
    }

    public class L10nLanguage
    {
        public string LanguageName { get; set; }
        public string Locale { get; set; }
        public List<string> PluralizationRules { get; set; } = new List<string>();
        public string PluralRule { get; set; }
        internal AstPluralRule AstPluralRule { get; set; }

        public ConcurrentDictionary<string, L10nPhraseObject> Phrases { get; set; } = new ConcurrentDictionary<string, L10nPhraseObject>();
    }
}
