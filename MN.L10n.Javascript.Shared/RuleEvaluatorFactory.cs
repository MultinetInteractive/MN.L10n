using Microsoft.Extensions.Caching.Memory;

namespace MN.L10n.Javascript
{
    public class RuleEvaluatorFactory
    {
        public static string CreateJavascriptRuleEvaluator(string language, IMemoryCache cache, bool minified)
        {
            L10nLanguage l10nItem = null;
            var cacheKey = "__l10n_" + language;

            if (cache != null)
            {
                if (cache.TryGetValue(cacheKey, out var l10nObjItem))
                {
                    l10nItem = l10nObjItem as L10nLanguage;
                }
            }

            if (l10nItem == null)
            {
                l10nItem = L10n.GetL10nLanguage(language);

                if (cache != null)
                {
                    cache.Set(cacheKey, l10nItem);
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

        public static string CreateJavascriptRuleEvaluator(IMemoryCache cache, bool minified)
        {
            var language = L10n.GetLanguage();
            return CreateJavascriptRuleEvaluator(language, cache, minified);
        }
    }
}
