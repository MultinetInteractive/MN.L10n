using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MN.L10n.Javascript
{
    public static class RuleEvaluatorFactory
    {
        public static string CreateJavascriptRuleEvaluator(string language, bool minified, bool includeTranslations = true)
        {
            L10nLanguage l10nItem = L10n.GetL10nLanguage(language);

            Newtonsoft.Json.JsonSerializerSettings jsOptions = new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat
            };
            if (minified)
                jsOptions = new Newtonsoft.Json.JsonSerializerSettings
                {
                    DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat
                };

            var jsL10n = includeTranslations
                ? l10nItem
                : new L10nLanguage
                {
                    Locale = l10nItem.Locale,
                    Phrases = new ConcurrentDictionary<string, L10nPhraseObject>(),
                    LanguageName = l10nItem.LanguageName,
                    PluralizationRules = l10nItem.PluralizationRules,
                    PluralRule = l10nItem.PluralRule
                };

            return "window.l10n = " + Newtonsoft.Json.JsonConvert.SerializeObject(jsL10n, jsOptions) + ";" +
                   "window.l10n.ruleEvaluator = function(n) { return ~~(" + l10nItem.PluralRule + "); };";
        }

        public static string CreateJavascriptRuleEvaluator(bool minified, bool includeTranslations = true)
        {
            var language = L10n.GetLanguage();
            return CreateJavascriptRuleEvaluator(language, minified, includeTranslations);
        }
    }
}
