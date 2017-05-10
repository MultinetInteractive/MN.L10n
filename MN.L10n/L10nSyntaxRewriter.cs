using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MN.L10n
{
	public class PhrasesRewriter : CSharpSyntaxRewriter
	{
		internal Dictionary<string, SyntaxNode> _PhraseKeys = new Dictionary<string, SyntaxNode>();
		internal List<SyntaxNode> _Class = new List<SyntaxNode>();
		internal L10n _phrases;
		internal Dictionary<string, Dictionary<object, L10nPhraseObject>> _phraseDic = new Dictionary<string, Dictionary<object, L10nPhraseObject>>();
		internal string LanguageIdentifier;
		internal string _BuildNumber;

		internal List<string> unusedPhrases = new List<string>();
		public PhrasesRewriter(string className, string languageIdentifier, L10n phraseRepo, string buildNumber, params string[] methods) : base()
		{
			if (!string.IsNullOrWhiteSpace(className))
				phraseClassName = className.Trim();
			LanguageIdentifier = languageIdentifier.Trim();
			_phrases = phraseRepo;

			_BuildNumber = buildNumber;

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

			if (methods.Length > 0)
				PhraseMethods = methods;
		}

		public bool SavePhrasesToFile()
		{
			foreach (var unused in unusedPhrases)
			{
				if (_phrases.Phrases[unused].LatestBuildUsage != _BuildNumber)
				{
					_phrases.Phrases.Remove(unused);
				}
			}

			foreach (var _ph in _phrases.Phrases.Keys)
			{
				_phrases.Phrases[_ph].LatestBuildUsage = _BuildNumber;
			}

			return L10n.SaveDataProvider();
		}

		internal string phraseClassName = "Phrase" + Guid.NewGuid().ToString().Replace("-", "");
		internal string[] PhraseMethods = new string[] { "_s", "_m" };
		public override SyntaxNode Visit(SyntaxNode node)
		{
			if (node is InvocationExpressionSyntax)
			{
				var inv = node as InvocationExpressionSyntax;
				if (PhraseMethods.Contains(inv.Expression.ToString()))
				{
					var arguments = inv.ArgumentList.Arguments;
					var propertyName = inv.Expression.ToString().Replace(".", "_") + "_" + Guid.NewGuid().ToString().Replace("-", "");
					var property = _AddProperty(propertyName, arguments);

					if (_PhraseKeys.ContainsKey(inv.ToString()))
					{
						return _PhraseKeys[inv.ToString()];
					}

					_Class.Add(property);
					var objItem = arguments.Where(a => a.Expression is AnonymousObjectCreationExpressionSyntax || a.Expression is InvocationExpressionSyntax);
					var argList = SeparatedList(objItem);
					var n = InvocationExpression(IdentifierName($"{phraseClassName}.{propertyName}"), ArgumentList(argList)).NormalizeWhitespace();
					if (!_PhraseKeys.ContainsKey(inv.ToString()))
						_PhraseKeys[inv.ToString()] = n;
					return n;
				}
			}

			return base.Visit(node);
		}

		internal SyntaxNode _AddProperty(string propName, SeparatedSyntaxList<ArgumentSyntax> args)
		{
			LiteralExpressionSyntax phrase = args.FirstOrDefault(a => a.Expression is LiteralExpressionSyntax)?.Expression as LiteralExpressionSyntax;

			var phraseText = phrase.Token.ValueText ?? "";
			var arguments = (args.FirstOrDefault(a => a.Expression is AnonymousObjectCreationExpressionSyntax)?.Expression as AnonymousObjectCreationExpressionSyntax)?.Initializers.Select(i => i.NameEquals.Name.Identifier.ValueText) ?? new List<string>();

			bool isMarkDown = propName.StartsWith("_m");
			bool isPluralized = arguments.Any(i => i == "__count");

			if (!_phrases.Phrases.ContainsKey(phraseText))
			{
				_phrases.Phrases.Add(phraseText, new L10nPhrase());
			}
			else
			{
				_phrases.Phrases[phraseText].Usages++;
			}

			if (unusedPhrases.Contains(phraseText))
			{
				unusedPhrases.Remove(phraseText);
			}

			if (isMarkDown)
			{
				phraseText = _phrases.ConvertFromMarkdown(phraseText);
			}

			SyntaxTrivia argToken =
			Trivia(
				SkippedTokensTrivia()
				.WithTokens(
					TokenList(
						Literal(phraseText),
						Token(SyntaxKind.CommaToken),
						Identifier("arg")
					)
				)
			);
			if (phrase == null)
			{
				argToken =
				Trivia(
					SkippedTokensTrivia()
					.WithTokens(
						TokenList(
							Token(SyntaxKind.ReturnKeyword),
							Identifier("FormatNamed"),
							Token(SyntaxKind.OpenParenToken),
							Identifier("arg"),
							Token(SyntaxKind.CloseParenToken)
						)
					)
				);
			}
			else
			{
				if (_phraseDic.ContainsKey(phrase.Token.ValueText))
				{
					var tokens = new List<SyntaxToken>();
					if (isPluralized)
					{
						tokens.AddRange(ParseTokens("var count = MN.L10n.L10n.GetCount(arg);"));
					}
					tokens.AddRange(ParseTokens("switch(" + LanguageIdentifier + ") {"));
					foreach (var langItem in _phraseDic[phrase.Token.ValueText])
					{
						if (!langItem.Value.r.ContainsKey("x"))
							continue;
						var key = langItem.Key;
						if (key is string)
							key = "\"" + key + "\"";
						tokens.AddRange(ParseTokens("case " + key + ":"));
						if (isPluralized)
						{
							foreach (var r in langItem.Value.r.Where(k => k.Key != "x"))
							{
								tokens.AddRange(ParseTokens("if(count == " + r.Key + ") { return MN.L10n.L10n.FormatNamed("));
								tokens.Add(Literal(langItem.Value.r[r.Key]));
								tokens.AddRange(ParseTokens("); }"));
							}
							tokens.AddRange(ParseTokens("return MN.L10n.L10n.FormatNamed("));
							tokens.Add(Literal(langItem.Value.r["x"]));
							tokens.AddRange(ParseTokens(", arg);"));
						}
						else
						{
							tokens.AddRange(ParseTokens("return MN.L10n.L10n.FormatNamed("));
							tokens.Add(Literal(langItem.Value.r["x"]));
							tokens.AddRange(ParseTokens(", arg);"));
						}

					}
					tokens.AddRange(ParseTokens("default: return MN.L10n.L10n.FormatNamed("));
					tokens.Add(Literal(phraseText));
					tokens.AddRange(ParseTokens(", arg);"));
					tokens.Add(ParseToken("}"));
					argToken = Trivia(
						SkippedTokensTrivia()
						.WithTokens(
							TokenList(tokens)
						)
					);
				}
				else
				{
					//_phrases.Phrases.Add(phrase.Token.ValueText, new L10nPhrase { });
					var tokens = new List<SyntaxToken>();
					if (isPluralized)
					{

					}
					argToken =
					Trivia(
						SkippedTokensTrivia()
						.WithTokens(
							TokenList(
								Token(SyntaxKind.ReturnKeyword),
								Identifier("FormatNamed"),
								Token(SyntaxKind.OpenParenToken),
								Literal(phraseText),
								Token(SyntaxKind.CommaToken),
								Identifier("arg"),
								Token(SyntaxKind.CloseParenToken)
							)
						)
					);
				}
			}
			var p = IdentifierName(
				MissingToken(
					TriviaList(),
					SyntaxKind.IdentifierToken,
					TriviaList(
						new[] {
						Trivia(
							SkippedTokensTrivia()
							.WithTokens(
								TokenList(
									Token(SyntaxKind.PublicKeyword),
									Token(SyntaxKind.StaticKeyword),
									Token(SyntaxKind.StringKeyword),
									Identifier(propName),
									Token(SyntaxKind.OpenParenToken),
									Token(SyntaxKind.ObjectKeyword),
									Identifier("arg"),
									Token(SyntaxKind.EqualsToken),
									Identifier("null"),
									Token(SyntaxKind.CloseParenToken),
									Token(SyntaxKind.OpenBraceToken)
								)
							)
						),
						argToken,
						Trivia(
							SkippedTokensTrivia()
							.WithTokens(
								TokenList(
									Token(SyntaxKind.SemicolonToken),
									Token(SyntaxKind.CloseBraceToken)
								)
							)
						)
						}
				)
			)
		).NormalizeWhitespace();
			return p;
		}
		public SyntaxNode GetPhraseStrings()
		{
			var root = ParseSyntaxTree(@"
public static partial class " + phraseClassName + @"
{
	" + string.Join("\n\t", _Class.Select(n => n.ToFullString())) + @"
}").GetRoot();
			return root;
		}
	}
}
