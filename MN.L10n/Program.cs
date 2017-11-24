using MN.L10n.FileProviders;
using MN.L10n.NullProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MN.L10n
{
    class Program
    {
		static int Main(string[] args)
		{
			string projectFolder = string.Empty;
			if (args.Length == 0)
			{
				throw new ArgumentNullException("projectFolder");
			}
			else
			{
				projectFolder = args[0];
			}

			Stopwatch stw = new Stopwatch();
			var fi = new FileInfo(projectFolder);
			var baseDir = fi.Directory;
			var sourceDir = Environment.CurrentDirectory;
			Console.WriteLine("info l10n: L10n - beginning work: " + sourceDir);

			stw.Start();

			L10nConfig config = new L10nConfig();

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

			if (config != null && config.PreventBuildTask)
			{
				Console.WriteLine("info l10n: L10n build task cancelled by config file");
				return 0;
			}

			L10n PhraseInstance = L10n.CreateInstance(
				new NullLanguageProvider(),
				new FileDataProvider(solutionDir)
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
				"/packages", "\\packages",
				".min.js", ".css"
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

			var phraseRewriter = new PhrasesRewriter(PhraseInstance);

			var parser = new L10nParser();

			foreach (var file in fileList.Distinct())
			{
				var fileContents = File.ReadAllText(file);
				var shortFile = file.Replace(solutionDir, "");
				var invocations = parser.Parse(fileContents);

				foreach (var _phrase in invocations)
				{
					if (!PhraseInstance.Phrases.ContainsKey(_phrase))
					{
						PhraseInstance.Phrases.Add(_phrase, new L10nPhrase() { Sources = new List<string> { shortFile } });
					}
					else
					{
						PhraseInstance.Phrases[_phrase].Usages++;
						if (!PhraseInstance.Phrases[_phrase].Sources.Contains(shortFile))
						{
							PhraseInstance.Phrases[_phrase].Sources.Add(shortFile);
						}
					}

					if (phraseRewriter.unusedPhrases.Contains(_phrase))
					{
						phraseRewriter.unusedPhrases.Remove(_phrase);
					}
				}
				if (config.ShowDetailedLog) Console.WriteLine("info l10n: Checked phrases in: " + shortFile + ", found " + invocations.Count + " phrases");
			};

			phraseRewriter.SavePhrasesToFile();
			stw.Stop();
			Console.WriteLine("info l10n: Spent " + stw.Elapsed + " running L10n, found " + PhraseInstance.Phrases.Count + " phrases");

			return 0;
		}
    }
}
