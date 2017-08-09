namespace MN.L10n.NullProviders
{
	public class NullLanguageProvider : IL10nLanguageProvider
	{
		private string Language { get; set; }
		public NullLanguageProvider(string lang = "sv-SE")
		{
			Language = lang;
		}
		public string GetLanguage()
		{
			return Language;
		}
	}
}
