using Microsoft.Ajax.Utilities;
using Microsoft.CodeAnalysis;
using MN.L10n.FileProviders;
using MN.L10n.NullProviders;
using StackExchange.Precompilation;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MN.L10n
{
	public class CodeCompilerModule : ICompileModule
	{
		public L10n PhraseInstance { get; set; }
		public void AfterCompile(AfterCompileContext context) { }

		public void BeforeCompile(BeforeCompileContext context)
		{
			var baseDir = new DirectoryInfo(context.Arguments.BaseDirectory);

			while (!baseDir.GetFiles("*.sln").Any())
			{
				baseDir = baseDir.Parent;
			}

			var solutionDir = baseDir.FullName;

			var bpEnvName = solutionDir + "__l10n_build";
			
			var bpIdentifier = Environment.GetEnvironmentVariable(bpEnvName, EnvironmentVariableTarget.Machine);
			if (string.IsNullOrWhiteSpace(bpIdentifier) || !IsValidBuildIdentifier(bpIdentifier))
			{
				bpIdentifier = Guid.NewGuid().ToString() + "|" + DateTime.Now.ToString();
				Environment.SetEnvironmentVariable(bpEnvName, bpIdentifier, EnvironmentVariableTarget.Machine);
			}

			context.Diagnostics.Add(
						Diagnostic.Create(
							new DiagnosticDescriptor("L10n", "TEST", "BuildIdentifier: " + bpIdentifier, "Translated", DiagnosticSeverity.Info, true),
							Location.None));
			
			PhraseInstance = L10n.CreateInstance(new NullLanguageProvider("1"), new FileDataProvider(solutionDir));

			var validExtensions = new[] { ".aspx", ".ascx", ".js", ".jsx" };
			var fileList = Directory.EnumerateFiles(context.Arguments.BaseDirectory, "*.*", SearchOption.AllDirectories)
			.Where(f => validExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

			var methods = new[] 
			{
				"_s",
				"_m",
				"MN.L10n.L10n._s",
				"MN.L10n.L10n._m"
			};

			var phraseRewriter = new PhrasesRewriter("L10n_rw", "MN.L10n.L10n.GetLanguage()", PhraseInstance, bpIdentifier, methods);

			var jsp = new JSParser();
			var set = new CodeSettings { IgnoreAllErrors = false, MinifyCode = false, OutputMode = OutputMode.MultipleLines, BlocksStartOnSameLine = BlockStart.UseSource };

			var r = new Regex(@"(?:MN\.)?(?:L10n\.)?(?:L10n\.)?_[sm]\(['""](.*)['""](?:,)?(.*?)\)", RegexOptions.Compiled);
			foreach(var file in fileList)
			{
				if (file.EndsWith(".js"))
				{
					var m = r.Matches(File.ReadAllText(file));
					if (m.Count > 0)
					{

						foreach(var lang in PhraseInstance.Languages)
						{
							var jsRewriter = new JSL10nTreeVisitor(PhraseInstance, lang);
							var origSource = File.ReadAllText(file);
							var astBlock = jsp.Parse(origSource, set);
							jsRewriter.Visit(astBlock);
							StringBuilder _code = new StringBuilder();
							using (StringWriter sw = new StringWriter(_code))
							{
								OutputVisitor.Apply(sw, astBlock, set);
							}
							var code = _code.ToString();
							if (code != origSource)
							{
								var jsExt = file.LastIndexOf(".js");

								var newFileName = string.Format("{0}-{1}.js", file.Substring(0, jsExt), lang);

								File.WriteAllText(newFileName, code);
								foreach (var up in jsRewriter.unusedPhrases)
								{
									if (phraseRewriter.unusedPhrases.Contains(up))
									{
										phraseRewriter.unusedPhrases.Remove(up);
									}
								}
							}
						};
						context.Diagnostics.Add(
						Diagnostic.Create(
							new DiagnosticDescriptor("L10n", "TEST", "Checked phrases in: " + file, "Translated", DiagnosticSeverity.Info, true),
							Location.None));
					}
				}
				else
				{
					var m = r.Matches(File.ReadAllText(file));
					if (m.Count > 0)
					{
						foreach (Match match in m)
						{
							var phraseText = match.Groups[1].Value.Trim();
							var args = match.Groups[2].Value.Trim();

							if (!PhraseInstance.Phrases.ContainsKey(phraseText))
							{
								PhraseInstance.Phrases.Add(phraseText, new L10nPhrase() { LatestBuildUsage = bpIdentifier });
							}
							else
							{
								PhraseInstance.Phrases[phraseText].Usages++;
							}

							if (phraseRewriter.unusedPhrases.Contains(phraseText))
							{
								phraseRewriter.unusedPhrases.Remove(phraseText);
							}
						}
					}
				}
			};

			foreach(var st in context.Compilation.SyntaxTrees)
			{
				phraseRewriter._Class.Clear();
				var model = context.Compilation.GetSemanticModel(st);
				var rootNode = st.GetRoot();
				var rewritten = phraseRewriter.Visit(rootNode);
				if (rootNode != rewritten)
				{
					var phraseClass = phraseRewriter.GetPhraseStrings();
					var rewrittenRoot = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(rewritten.ToFullString() + phraseClass.ToFullString()).GetRoot();

					context.Compilation = context.Compilation.ReplaceSyntaxTree(st,
						st.WithRootAndOptions(rewrittenRoot, st.Options));

					context.Diagnostics.Add(
						Diagnostic.Create(
							new DiagnosticDescriptor("L10n", "TEST", "Replaced " + phraseRewriter._Class.Count + " phrases in: " + st.FilePath, "Translated", DiagnosticSeverity.Info, true),
							Location.None));
				}
			}
			phraseRewriter.SavePhrasesToFile();
		}

		private bool IsValidBuildIdentifier(string bpIdentifier)
		{
			var parts = bpIdentifier.Split('|');
			if (parts.Length != 2)
			{
				return false;
			}

			var ts = DateTime.Now - DateTime.Parse(parts[1]);
			return ts.TotalMinutes < 2;
		}
	}
}
