using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MN.L10n.NullProviders
{
	public class NullLanguageProvider : IL10nLanguageProvider
	{
		public string GetLanguage()
		{
			return "sv-SE";
		}
	}
}
