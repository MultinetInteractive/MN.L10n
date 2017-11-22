namespace MN.L10n.Javascript
{
    public class RuleEvaluatorFactory
    {
        public static string CreateJavascriptRuleEvaluator(string language, System.Web.Caching.Cache cache, bool minified)
        {
            L10nLanguage l10nItem = null;
            if (cache != null)
            {
                l10nItem = cache["__l10n_" + language] as L10nLanguage;
            }

            if (l10nItem == null)
            {
                l10nItem = L10n.GetL10nLanguage(language);

                if (cache != null)
                {
                    cache["__l10n_" + language] = l10nItem;
                }
            }

			Jil.Options jsOptions = Jil.Options.ISO8601PrettyPrint;
			if (minified)
				jsOptions = Jil.Options.ISO8601;

            return "window.l10n = " + Jil.JSON.Serialize(l10nItem, jsOptions) + ";" +
                   "window.l10n.ruleEvaluator = function(n) { return ~~(" + l10nItem.PluralRule + "); };";
        }

        public static string CreateJavascriptRuleEvaluator(System.Web.Caching.Cache cache, bool minified)
        {
            var language = L10n.GetLanguage();
            return CreateJavascriptRuleEvaluator(language, cache, minified);
        }
    }
}
