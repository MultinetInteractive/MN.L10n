using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MN.L10n.Javascript
{
    public class RuleEvaluatorFactory
    {
        public static string CreateJavascriptRuleEvaluator(string language, System.Web.Caching.Cache cache)
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

            return "window.l10n = " + Jil.JSON.Serialize(l10nItem, Jil.Options.ISO8601PrettyPrint) + ";" +
                   "window.l10n.ruleEvaluator = function(n) { return ~~(" + l10nItem.PluralRule + "); };";
        }

        public static string CreateJavascriptRuleEvaluator(System.Web.Caching.Cache cache)
        {
            var language = L10n.GetLanguage();
            return CreateJavascriptRuleEvaluator(language, cache);
        }
    }
}
