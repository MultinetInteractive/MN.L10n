using System.Web.Mvc;

namespace MN.L10n.Mvc
{
	public class L10nWebView<T> : WebViewPage<T>
	{
		public string _s(string phrase, object args = null) => L10n._s(phrase, args);

		public string _m(string phrase, object args = null) => L10n._m(phrase, args);

		public override void Execute()
		{
			
		}
	}
}
