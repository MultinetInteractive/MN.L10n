using NGettext.Plural;
using System.Collections.Generic;

namespace MN.L10n
{
	public class L10nLanguage
	{
		public string LanguageName { get; set; }
		public string Locale { get; set; }
		public List<string> PluralizationRules { get; set; } = new List<string>();
		public string PluralRule { get; set; }
		internal AstPluralRule AstPluralRule { get; set; }
		public Dictionary<string, L10nPhraseObject> Phrases { get; set; } = new Dictionary<string, L10nPhraseObject>();
	}
}
