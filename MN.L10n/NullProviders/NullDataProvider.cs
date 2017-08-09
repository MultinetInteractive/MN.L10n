using System.Collections.Generic;

namespace MN.L10n.NullProviders
{
	public class NullDataProvider : IL10nDataProvider
	{
		public L10n LoadL10n()
		{
			return new L10n
			{
				Languages = new List<string> { "sv-SE" },
				Phrases = new Dictionary<string, L10nPhrase>()
			};
		}

		public bool SaveL10n(L10n l10n)
		{
			return true;
		}
	}
}
