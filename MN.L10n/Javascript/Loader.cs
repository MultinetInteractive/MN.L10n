using System;

namespace MN.L10n.Javascript
{
    public static class Loader
    {
		public static string LoadL10nJavascript(string rootRelativePath, Func<string, bool> checkFile)
		{
			var lang = L10n.GetLanguage();
			var jsExt = rootRelativePath.LastIndexOf(".js");

			var newFileName = string.Format("{0}-{1}.js", rootRelativePath.Substring(0, jsExt), lang);
			if (!checkFile(newFileName))
			{
				return rootRelativePath;
			}

			return newFileName;
		}
	}
}
