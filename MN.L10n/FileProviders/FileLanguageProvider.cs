using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MN.L10n.FileProviders
{
	public class FileLanguageProvider : IL10nLanguageProvider
	{
		private string Language { get; set; }
		public FileLanguageProvider(string language)
		{
			Language = language;
		}

		public string GetLanguage()
		{
			return Language;
		}
	}
}
