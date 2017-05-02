using System.Collections.Generic;

namespace MN.L10n
{
	public class L10n
	{
		public List<string> Languages { get; set; } = new List<string>();
		public Dictionary<string, L10nPhrase> Phrases { get; set; } = new Dictionary<string, L10nPhrase>();
	}
}
