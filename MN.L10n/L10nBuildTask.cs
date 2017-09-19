using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MN.L10n.FileProviders;
using MN.L10n.NullProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MN.L10n
{
	public class L10nBuildTask : Task
	{
		public override bool Execute()
		{
			var fi = new FileInfo(BuildEngine.ProjectFileOfTaskNode);
			
			var baseDir = fi.Directory;
			Log.LogMessage(MessageImportance.High, "info l10n: L10n - beginning work: " + baseDir.FullName);
			var msgs = new List<string>
			{
				"CurrentDir-" + System.IO.Directory.GetCurrentDirectory(),
				"EnvCurrent-" + Environment.CurrentDirectory
			};

			foreach (var msg in msgs)
			{
				Log.LogMessage(MessageImportance.High, "info l10n: " + msg);
			}
			Debugger.Break();
			Stopwatch stw = new Stopwatch();
			stw.Start();
			
			L10nConfig config = null;

			while (!baseDir.GetFiles("*.sln").Any())
			{
				baseDir = baseDir.Parent;
			}

			var solutionDir = baseDir.FullName;

			var cfgFile = baseDir.GetFiles(".l10nconfig").FirstOrDefault();
			if (cfgFile != null)
			{
				config = Jil.JSON.Deserialize<L10nConfig>(File.ReadAllText(cfgFile.FullName));
			}

			L10n PhraseInstance = L10n.CreateInstance(
				new NullLanguageProvider("1"), 
				new FileDataProvider(solutionDir), 
				new FileResolver()
			);

			var validExtensions = new[] { ".aspx", ".ascx", ".js", ".jsx", ".cs", ".cshtml", ".ts", ".tsx" };

			var defaultIgnorePaths = new[] {
				"/.git", "\\.git",
				"/node_modules", "\\node_modules",
				"/.vscode", "\\.vscode",
				"/.idea", "\\.idea",
				"/.vs", "\\.vs",
				"/bin", "\\bin",
				"/obj", "\\obj",
				".dll", ".designer.cs",
				"/packages", "\\packages"
			};
			
			List<string> fileList = new List<string>();

			if (config != null)
			{
				if (config.IncludePatterns.Count == 0)
				{
					fileList = Directory.EnumerateFiles(solutionDir, "*.*", SearchOption.AllDirectories)
					.Where(f => validExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
					.Where(f => !defaultIgnorePaths.Any(ign => f.ToLower().Contains(ign)))
					.ToList();
				}

				foreach (var pattern in config.IncludePatterns)
				{
					fileList.AddRange(
						Glob.Glob.ExpandNames(solutionDir + pattern)
						.Where(f => !defaultIgnorePaths.Any(ign => f.ToLower().Contains(ign)))
					);
				}

				foreach (var pattern in config.ExcludePatterns)
				{
					var match = Glob.Glob.ExpandNames(solutionDir + pattern);
					foreach (var m in match)
					{
						fileList.Remove(m);
					}
				}
			}
			else
			{
				fileList = Directory.EnumerateFiles(solutionDir, "*.*", SearchOption.AllDirectories)
				.Where(f => validExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
				.Where(f => !defaultIgnorePaths.Any(ign => f.ToLower().Contains(ign)))
				.ToList();
			}
			
			var methods = new[]
			{
				"_s",				"_m",
				"MN.L10n.L10n._s",	"MN.L10n.L10n._m"
			};

			var phraseRewriter = new PhrasesRewriter("L10n_rw", "MN.L10n.L10n.GetLanguage()", PhraseInstance, methods);

			var noParam = new Regex(@"(?:MN\.)?(?:L10n\.)?(?:L10n\.)?_[sm]\(['""](.*?)['""]\)", RegexOptions.Compiled);
			var withParam = new Regex(@"(?:MN\.)?(?:L10n\.)?(?:L10n\.)?_[sm]\(['""](.*?)['""],.*?{(.*?)}\)", RegexOptions.Compiled);

			var findParam = new Regex(@"(\$[a-zA-Z_]*?\$)", RegexOptions.Compiled);

			foreach (var file in fileList.Distinct())
			{
				var fileContents = File.ReadAllText(file);
				var m = noParam.Matches(fileContents).Cast<Match>().ToList();
				m.AddRange(withParam.Matches(fileContents).Cast<Match>().ToList());
				if (m.Count > 0)
				{
					foreach (Match match in m)
					{
						var phraseText = match.Groups[1].Value.Trim();
						var args = match.Groups[2].Value.Trim();

						if (match.Groups.Count == 2)
						{
							// Here we check if the regex matched too much, until we get a better regex...
							var hasParams = findParam.Matches(match.Value).Cast<Match>().ToList();
							if (hasParams.Any())
							{
								continue;
							}
						}

						if (!PhraseInstance.Phrases.ContainsKey(phraseText))
						{
							PhraseInstance.Phrases.Add(phraseText, new L10nPhrase() { });
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
				Log.LogMessage(MessageImportance.High, "info l10n: Checked phrases in: " + file + ", found " + m.Count + " phrases");
			};

			phraseRewriter.SavePhrasesToFile();
			stw.Stop();
			Log.LogMessage(MessageImportance.High, "info l10n: Spent " + stw.Elapsed + " running L10n, found " + PhraseInstance.Phrases.Count + " phrases");

			return true;
		}
	}
}
