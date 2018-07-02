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

        public static string CreateJavascriptRuleEvaluator(System.Web.Caching.Cache cache, bool minified)
        {
            var language = L10n.GetLanguage();
            return CreateJavascriptRuleEvaluator(language, cache, minified);
        }
    }
}
