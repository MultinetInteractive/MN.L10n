using Microsoft.Ajax.Utilities;
using Microsoft.CodeAnalysis;
using MN.L10n.FileProviders;
using MN.L10n.NullProviders;
using StackExchange.Precompilation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			bool isBuildEnvironment = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("is_build_environment"));

			var runPhrase = false;
			try { runPhrase = context.Arguments.CompilationOptions.OptimizationLevel == OptimizationLevel.Release; } catch { }
			if (!runPhrase) return;
			Debugger.Break();
			Stopwatch stw = new Stopwatch();
			stw.Start();

			var baseDir = new DirectoryInfo(context.Arguments.BaseDirectory);

			L10nConfig config = null;

			while (!baseDir.GetFiles("*.sln").Any())
			{
				baseDir = baseDir.Parent;
			}

			var solutionDir = baseDir.FullName;
			var hashPath = Path.Combine(solutionDir, "l10n-hash.json");
			if (!FileHashHelper.HashesLoaded)
			{
				FileHashHelper.LoadFileHashes(hashPath);
			}

			var cfgFile = baseDir.GetFiles(".l10nconfig").FirstOrDefault();
			if (cfgFile != null)
			{
				config = Jil.JSON.Deserialize<L10nConfig>(File.ReadAllText(cfgFile.FullName));
			}

			var bpEnvName = solutionDir + "__l10n_build";

			var bpIdentifier = Environment.GetEnvironmentVariable(bpEnvName, EnvironmentVariableTarget.Machine);
			if (string.IsNullOrWhiteSpace(bpIdentifier) || !IsValidBuildIdentifier(bpIdentifier))
			{
				bpIdentifier = Guid.NewGuid().ToString() + "|" + DateTime.Now.ToString();
				Environment.SetEnvironmentVariable(bpEnvName, bpIdentifier, EnvironmentVariableTarget.Machine);
			}

			Console.WriteLine("info l10n: BuildIdentifier: " + bpIdentifier);

			L10n PhraseInstance = L10n.CreateInstance(new NullLanguageProvider("1"), new FileDataProvider(solutionDir), new FileResolver());

			var validExtensions = new[] { ".aspx", ".ascx", ".js", ".jsx" };

			var defaultIgnorePaths = new[] {
				"/.git", "\\.git",
				"/node_modules", "\\node_modules",
				"/.vscode", "\\.vscode",
				"/.idea", "\\.idea",
				"/.vs", "\\.vs",
				"/bin", "\\bin",
				"/obj", "\\obj",
			};


			List<string> fileList = new List<string>();

			if (config != null)
			{
				if (config.IncludePatterns.Count == 0)
				{
					fileList = Directory.EnumerateFiles(context.Arguments.BaseDirectory, "*.*", SearchOption.AllDirectories)
					.Where(f => validExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
					.Where(f => !defaultIgnorePaths.Any(ign => f.ToLower().Contains(ign)))
					.ToList();
				}

				foreach (var pattern in config.IncludePatterns)
				{
					fileList.AddRange(
						Glob.Glob.ExpandNames(context.Arguments.BaseDirectory + pattern)
						.Where(f => !defaultIgnorePaths.Any(ign => f.ToLower().Contains(ign)))
					);
				}

				foreach (var pattern in config.ExcludePatterns)
				{
					Glob.Glob.ExpandNames(context.Arguments.BaseDirectory + pattern).ForEach(s => fileList.Remove(s));
				}
			}
			else
			{
				fileList = Directory.EnumerateFiles(context.Arguments.BaseDirectory, "*.*", SearchOption.AllDirectories)
				.Where(f => validExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
				.Where(f => !defaultIgnorePaths.Any(ign => f.ToLower().Contains(ign)))
				.ToList();
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
			if (!isBuildEnvironment)
			{
				foreach (var file in fileList.Distinct())
				{
					var fileContents = File.ReadAllText(file);
					var fileHash = FileHashHelper.GetHash(fileContents);
					if (FileHashHelper.FileHashes.ContainsKey(file) && FileHashHelper.FileHashes[file] == fileHash)
						continue;

					FileHashHelper.FileHashes[file] = fileHash;
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
							Console.WriteLine("info l10n: Checked phrases in (JavaScript): " + file);
						}
					}
					else
					{
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
							Console.WriteLine("info l10n: Checked phrases in: " + file);
						}
					}
				};
			}
			else
			{
				Console.WriteLine("info l10n: Skipping content files, build server");
			}

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

					Console.WriteLine("info l10n: Replaced " + phraseRewriter._Class.Count + " phrases in: " + st.FilePath);
				}
			}
			phraseRewriter.SavePhrasesToFile();
			FileHashHelper.SaveFileHashes(hashPath);
			stw.Stop();
			Console.WriteLine("info l10n: Spent " + stw.Elapsed + " running L10n");
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
