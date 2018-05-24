using MN.L10n.NullProviders;
using System;

namespace MN.L10n.Providers.LanguageProviders
{
    public class SwitchableLanguageProvider : NullLanguageProvider, IL10nLanguageProvider
	{
		[ThreadStatic]
		public string Language = "sv-SE";

		public SwitchableLanguageProvider(string language)
		{
			Language = language;
		}

		public IDisposable LocalLanguageContext(string language)
		{
			IDisposable obj = new TransactionLanguage(this, language, Language);

			return obj;
		}

		public void SetLanguage(string language)
		{
			Language = language;
		}

		internal class TransactionLanguage : IDisposable
		{

			public TransactionLanguage(SwitchableLanguageProvider provider, string newLang, string prevLang)
			{
				Provider = provider;
				NewLang = newLang;
				PrevLang = prevLang;

				Provider.Language = NewLang;
			}
			private string NewLang;
			private string PrevLang;
			private SwitchableLanguageProvider Provider;
			public void Dispose()
			{
				Provider.Language = PrevLang;
			}
		}
	}
}
