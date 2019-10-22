namespace MN.L10n.Javascript
{
    public static class RuleEvaluatorFactory
    {
        public static string CreateJavascriptRuleEvaluator(string language, bool minified)
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

            return "window.l10n = " + Newtonsoft.Json.JsonConvert.SerializeObject(l10nItem, jsOptions) + ";" +
                   "window.l10n.ruleEvaluator = function(n) { return ~~(" + l10nItem.PluralRule + "); };";
        }

        public static string CreateJavascriptRuleEvaluator(bool minified)
        {
            var language = L10n.GetLanguage();
            return CreateJavascriptRuleEvaluator(language, minified);
        }
    }
}
