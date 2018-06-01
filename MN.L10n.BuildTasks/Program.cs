using MN.L10n.FileProviders;
using MN.L10n.NullProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MN.L10n.BuildTasks
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
				projectFolder = string.Join(" ", args[0]).TrimEnd(Path.DirectorySeparatorChar, '"');
			}

			Stopwatch stw = new Stopwatch();
			var fi = new DirectoryInfo(projectFolder);
			var baseDir = fi;
			var sourceDir = Environment.CurrentDirectory;

            Console.WriteLine(baseDir.FullName + ";");
            Console.WriteLine("info l10n: L10n - beginning work: " + sourceDir);

			stw.Start();

			L10nConfig config = new L10nConfig();

			while (!baseDir.EnumerateFiles().Any(x => x.Extension == ".sln" || x.Extension == ".l10nroot"))
			{
				baseDir = baseDir.Parent;
			}

			var solutionDir = baseDir.FullName;

            var lockFile = Path.Combine(solutionDir, ".l10nLock");

			var lockFileExists = File.Exists(lockFile);
			if (lockFileExists)
			{
				Console.WriteLine("info l10n: Lock file exists, exiting gracefully");
				return 0;
			}

			File.WriteAllText(lockFile, DateTime.UtcNow.ToString());
			try
			{

				var cfgFile = baseDir.GetFiles(".l10nconfig").FirstOrDefault();
				if (cfgFile != null)
				{
					config = Jil.JSON.Deserialize<L10nConfig>(File.ReadAllText(cfgFile.FullName));
				}

				if (config != null && config.PreventBuildTask)
				{
					Console.WriteLine("info l10n: L10n build task cancelled by config file");
					File.Delete(lockFile);
					return 0;
				}

				L10n PhraseInstance = L10n.CreateInstance(
					new NullLanguageProvider(),
					new FileDataProvider(solutionDir)
				);

				var validExtensions = new[] { ".aspx", ".ascx", ".js", ".jsx", ".cs", ".cshtml", ".ts", ".tsx", ".master", ".ashx", ".php" };

				var defaultIgnorePaths = new[] {
					"/.git/", "\\.git\\",
					"/node_modules/", "\\node_modules\\",
					"/.vscode/", "\\.vscode",
					"/.idea/", "\\.idea",
					"/.vs/", "\\.vs",
					"/bin/", "\\bin",
					"/obj/", "\\obj\\",
					"/packages/", "\\packages\\",
				};

				var defaultIgnoreExtensions = new[] {
					".min.js", ".css",
					".dll", ".designer.cs",
				};

				List<string> fileList = new List<string>();

				if (config != null)
				{
					if (config.IncludePatterns.Count == 0)
					{
						fileList = Directory.EnumerateFiles(solutionDir, "*.*", SearchOption.AllDirectories)
						.Where(f => validExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
						.Where(f => 
							!defaultIgnoreExtensions.Any(ign => f.ToLower().EndsWith(ign)) && 
							!defaultIgnorePaths.Any(ign => f.ToLower().Contains(ign))
						)
						.ToList();
					}
                    else
                    {
						var fileListWithIgnores = Directory.EnumerateFiles(solutionDir, "*.*", SearchOption.AllDirectories)
							.Where(f => !defaultIgnorePaths.Any(ign => f.ToLower().Contains(ign)));

						var filesListWithIncludes = Directory.EnumerateFiles(solutionDir, "*.*", SearchOption.AllDirectories)
							.Where(f => config.IncludePatterns.Any(p => f.ToLower().Contains(p.ToLower())));

						var combinedList = fileListWithIgnores.Union(filesListWithIncludes);

						fileList.AddRange(combinedList);
					}


					if(config.ExcludePatterns.Count > 0)
					{
					    fileList = fileList.Where(f => config.ExcludePatterns.All(p => !f.ToLower().Contains(p.ToLower()))).ToList();
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

				foreach (var p in PhraseInstance.Phrases)
				{
					p.Value.Sources = new List<string>();
				}

				foreach (var file in fileList.Distinct())
				{
					var fileContents = File.ReadAllText(file);
					var shortFile = file.Replace(solutionDir, "");
					var invocations = parser.Parse(fileContents);

					foreach (var _phrase in invocations)
					{
						if (!PhraseInstance.Phrases.ContainsKey(_phrase.Phrase))
						{
							PhraseInstance.Phrases.TryAdd(_phrase.Phrase, new L10nPhrase() { Sources = new List<string> { shortFile + ":" + _phrase.Row } });
						}
						else
						{
							PhraseInstance.Phrases[_phrase.Phrase].Usages++;
							if (!PhraseInstance.Phrases[_phrase.Phrase].Sources.Contains(shortFile + ":" + _phrase.Row))
							{
								PhraseInstance.Phrases[_phrase.Phrase].Sources.Add(shortFile + ":" + _phrase.Row);
							}
						}

						if (phraseRewriter.unusedPhrases.Contains(_phrase.Phrase))
						{
							phraseRewriter.unusedPhrases.Remove(_phrase.Phrase);
						}
					}
					if (config.ShowDetailedLog) Console.WriteLine("info l10n: Checked phrases in: " + shortFile + ", found " + invocations.Count + " phrases");
				};

				phraseRewriter.SavePhrasesToFile();
				stw.Stop();
				Console.WriteLine("info l10n: Spent " + stw.Elapsed + " running L10n, found " + PhraseInstance.Phrases.Count + " phrases");

                var dir = new DirectoryInfo(baseDir.FullName);
			    
                var files = dir.GetFiles();

			    var langRegex = new Regex("language-[^\\.]*\\.json");

			    var toCopy = new List<FileInfo>();
			    foreach (var file in files)
			    {
			        if (file.Name == "phrases.json" || file.Name == "languages.json" || langRegex.IsMatch(file.Name))
			        {
			            toCopy.Add(file);
			        }
			    }

			    var destDirName = Path.Combine(projectFolder, "L10n");

			    if (!Directory.Exists(destDirName))
			    {
			        Directory.CreateDirectory(destDirName);
			    }

			    Console.WriteLine($@"Copying phrase-files from {baseDir.FullName} to {destDirName}");
                
			    foreach (var file in toCopy)
			    {
			        file.CopyTo(Path.Combine(destDirName, file.Name), true);
			    }

			    Console.WriteLine($@"Files copied to {destDirName}");

                File.Delete(lockFile);
				return 0;
			}
			catch
			{
				File.Delete(lockFile);
				return -1;
			}
			finally
			{
				File.Delete(lockFile);
			}
		}
    }
}
