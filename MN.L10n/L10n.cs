using System;
using System.Collections.Generic;

namespace MN.L10n
{
	public class L10n
	{
		private IL10nDataProvider DataProvider { get; set; }
		private IL10nLanguageProvider LanguageProvider { get; set; }

		internal static L10n Instance;
		
		public static L10n CreateInstance(IL10nLanguageProvider langProvider, IL10nDataProvider dataProvider)
		{
			var l10n = dataProvider.LoadL10n();
			l10n.DataProvider = dataProvider;
			l10n.LanguageProvider = langProvider;
			Instance = l10n;
			return l10n;
		}
		public List<string> Languages { get; set; } = new List<string>();
		public Dictionary<string, L10nPhrase> Phrases { get; set; } = new Dictionary<string, L10nPhrase>();

		public static string _s(string phrase, object args = null)
		{
			if (Instance == null) throw new Exception("You must use L10n.CreateInstance(langProvider, dataProvider) to create an instance before using this.");
			return Instance.__getPhrase(phrase, args);
		}

		public static string _m(string phrase, object args = null)
		{
			if (Instance == null) throw new Exception("You must use L10n.CreateInstance(langProvider, dataProvider) to create an instance before using this.");

			var settings = CommonMark.CommonMarkSettings.Default.Clone();
			settings.OutputFormat = CommonMark.OutputFormat.Html;
			settings.RenderSoftLineBreaksAsLineBreaks = true;

			return CommonMark.CommonMarkConverter.Convert(Instance.__getPhrase(phrase), settings);
		}

		internal string __getPhrase(string phrase, object args = null)
		{
			if (!Phrases.ContainsKey(phrase))
			{
				Phrases.Add(phrase, new L10nPhrase());
			}

			return FormatNamed(phrase, args);
		}

		internal bool IsPluralized(object args = null)
		{
			if (args == null) return false;
			var t = args.GetType();
			foreach (var p in t.GetProperties())
			{
				if (p.Name == "__count") return true;
			}
			return false;
		}

		internal int getCount(object args = null)
		{
			if (args == null) return 0;
			var t = args.GetType();
			foreach (var p in t.GetProperties())
			{
				if (p.Name == "__count") return (int)p.GetValue(args);
			}
			return 0;
		}

		internal string FormatNamed(string formatString, object parameters = null)
		{
			if (parameters == null) return formatString;

			var t = parameters.GetType();
			var tmpVal = formatString;
			foreach (var p in t.GetProperties())
			{
				tmpVal = tmpVal.Replace("$" + p.Name + "$", p.GetValue(parameters).ToString());
			}
			return tmpVal;
		}
	}
}
