using Microsoft.Ajax.Utilities;
using Microsoft.CodeAnalysis;
using MN.L10n.FileProviders;
using MN.L10n.NullProviders;
using StackExchange.Precompilation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MN.L10n
{
	public class CodeCompilerModule : ICompileModule
	{
		public void AfterCompile(AfterCompileContext context) { }

		public void BeforeCompile(BeforeCompileContext context)
		{
			//var runPhrase = true;
			//try { runPhrase = context.Arguments.CompilationOptions.OptimizationLevel == OptimizationLevel.Debug; } catch { }
			//if (!runPhrase) return;

			var baseDir = new DirectoryInfo(context.Arguments.BaseDirectory);

			L10nConfig config = null;

			while (!baseDir.GetFiles("*.sln").Any())
			{
				var cfgFile = baseDir.GetFiles(".l10nconfig").FirstOrDefault();
				if (cfgFile != null)
				{
					config = Jil.JSON.Deserialize<L10nConfig>(File.ReadAllText(cfgFile.FullName));
					break;
				}
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

			L10n PhraseInstance = L10n.CreateInstance(new NullLanguageProvider("1"), new FileDataProvider(solutionDir));

			var validExtensions = new[] { ".aspx", ".ascx", ".js", ".jsx" };

			List<string> fileList = new List<string>();

			if (config != null)
			{
				if (config.IncludePatterns.Count == 0)
				{
					fileList = Directory.EnumerateFiles(context.Arguments.BaseDirectory, "*.*", SearchOption.AllDirectories)
					.Where(f => validExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase))).ToList();
				}

				foreach (var pattern in config.IncludePatterns)
				{
					fileList.AddRange(Glob.Glob.ExpandNames(pattern));
				}

				foreach (var pattern in config.ExcludePatterns)
				{
					Glob.Glob.ExpandNames(pattern).ForEach(s => fileList.Remove(s));
				}
			}
			else
			{
				fileList = Directory.EnumerateFiles(context.Arguments.BaseDirectory, "*.*", SearchOption.AllDirectories)
				.Where(f => validExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase))).ToList();
			}

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

			var noParam = new Regex(@"(?:MN\.)?(?:L10n\.)?(?:L10n\.)?_[sm]\(['""](.*?)['""]\)", RegexOptions.Compiled);
			var withParam = new Regex(@"(?:MN\.)?(?:L10n\.)?(?:L10n\.)?_[sm]\(['""](.*?)['""],.*?{(.*?)}\)", RegexOptions.Compiled);
			foreach (var file in fileList.Distinct())
			{
				// Vi kör bara översättning på rena javascriptfiler
				var fileContents = File.ReadAllText(file);
				if (file.EndsWith(".js"))
				{
					
					var m = noParam.Matches(fileContents).Cast<Match>().ToList();
					m.AddRange(withParam.Matches(fileContents).Cast<Match>().ToList());
					if (m.Count > 0)
					{

						foreach (var lang in PhraseInstance.Languages)
						{
							var jsRewriter = new JSL10nTreeVisitor(PhraseInstance, lang);
							var origSource = fileContents;
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
					// Här matchar vi bara antalet användningar av fraser
					var m = noParam.Matches(fileContents).Cast<Match>().ToList();
					m.AddRange(withParam.Matches(fileContents).Cast<Match>().ToList());
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

			// Här händer all magi med koden som ska kompileras
			foreach (var st in context.Compilation.SyntaxTrees)
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
