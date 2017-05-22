using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MN.L10n
{
	public class JSL10nTreeVisitor : TreeVisitor
	{
		internal L10n _phrases;
		internal Dictionary<string, Dictionary<object, L10nPhraseObject>> _phraseDic = new Dictionary<string, Dictionary<object, L10nPhraseObject>>();
		internal List<string> unusedPhrases = new List<string>();

		internal string _currentLang;

		public JSL10nTreeVisitor(L10n inst, string currentLanguage)
		{
			_phrases = inst;
			_currentLang = currentLanguage;

			var allPhrases = _phrases.Phrases.Keys;
			foreach (var p in allPhrases)
			{
				_phraseDic.Add(p, new Dictionary<object, L10nPhraseObject>());
				unusedPhrases.Add(p);
				_phrases.Phrases[p].Usages = 0;
			}

			foreach (var fLang in _phrases.LanguagePhrases)
			{
				var langKey = fLang.Key;
				var translatedPhrases = fLang.Value.Phrases;
				foreach (var trpr in translatedPhrases)
				{
					if (!_phraseDic.ContainsKey(trpr.Key))
					{
						_phraseDic.Add(trpr.Key, new Dictionary<object, L10nPhraseObject>());
					}
					_phraseDic[trpr.Key].Add(langKey, trpr.Value);
				}
			}
		}

		public string[] PhraseMethods = new[] { "_s", "_m" };

		public override void Visit(CallNode node)
		{
			base.Visit(node);
			var pm = (node.Function as Lookup)?.Name ?? "notValid";
			if (PhraseMethods.Contains(pm))
			{
				var args = node.Arguments;

				var phrase = (args[0] as ConstantWrapper)?.Value.ToString() ?? string.Empty;
				ObjectLiteral arg = null;
				if (args.Count > 1)
				{
					arg = args[1] as ObjectLiteral;
				}
				_GenerateJS invoker = GeneratePhraseJS;
				node.Parent.ReplaceChild(node, new JSL10nPhraseNode(node.Context.FlattenToStart(), invoker, pm, phrase, arg));
			}
		}

		public delegate string _GenerateJS(string method, string phrase, ObjectLiteral args = null);

		public string GeneratePhraseJS(string method, string phrase, ObjectLiteral args = null)
		{
			bool isMarkDown = method == "_m";
			bool isPluralized = (args?.Context.Code ?? "{}").Contains("__count");

			if (!_phrases.Phrases.ContainsKey(phrase))
			{
				_phrases.Phrases.Add(phrase, new L10nPhrase());
			}
			else
			{
				_phrases.Phrases[phrase].Usages++;
			}

			if (unusedPhrases.Contains(phrase))
			{
				unusedPhrases.Remove(phrase);
			}
			
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendLine("(function() {");
			if (args != null)
			{
				sb.AppendLine("var _args = " + (args?.Context.Code ?? "{}") + ";");
			}
			if (_phraseDic.ContainsKey(phrase) && _phraseDic[phrase].ContainsKey(_currentLang))
			{
				var langItem = _phraseDic[phrase][_currentLang];
				sb.AppendLine("var _phrase = '" + (isMarkDown ? _phrases.ConvertFromMarkdown(langItem.r["x"]) : langItem.r["x"]) + "';");
				
				foreach (var r in langItem.r.Where(k => k.Key != "x"))
				{
					sb.AppendLine("if(_args.__count == " + r.Key + ") { _phrase = ");
					sb.Append("'" + (isMarkDown ? _phrases.ConvertFromMarkdown(langItem.r[r.Key]) : langItem.r[r.Key]) + "'");
					sb.Append("; }");
				}
			}
			else
			{
				sb.AppendLine("var _phrase = '" + (isMarkDown ? _phrases.ConvertFromMarkdown(phrase) : phrase) + "';");
			}
			
			if(args != null)
			{
				sb.AppendLine("for(var p in _args) { if(_args.hasOwnProperty(p)) { _phrase = _phrase.replace('$' + p + '$', _args[p]); } }");
			}
			sb.AppendLine("return _phrase; })()");
			return sb.ToString();
		}
	}

	public class JSL10nPhraseNode : CustomNode
	{
		private ObjectLiteral _arg;
		private string _phrase;
		private Delegate generator;
		private string _meth;
		public JSL10nPhraseNode(Context context, Delegate getJs, string method, string phrase, ObjectLiteral arg) : base(context)
		{
			_phrase = phrase;
			_arg = arg;
			_meth = method;
			generator = getJs;
		}

		public override string ToCode()
		{
			return generator.DynamicInvoke(_meth, _phrase, _arg).ToString();
		}
	}
}
