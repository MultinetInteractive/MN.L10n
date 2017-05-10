using Microsoft.CodeAnalysis;
using MN.L10n.FileProviders;
using MN.L10n.NullProviders;
using StackExchange.Precompilation;
using System;
using System.IO;
using System.Linq;

namespace MN.L10n
{
	public class CodeCompilerModule : ICompileModule
	{
		public L10n PhraseInstance { get; set; }
		public void AfterCompile(AfterCompileContext context) { }

		public void BeforeCompile(BeforeCompileContext context)
		{
			var bpIdentifier = Environment.GetEnvironmentVariable("__l10n_build", EnvironmentVariableTarget.Machine);
			if (string.IsNullOrWhiteSpace(bpIdentifier) || !IsValidBuildIdentifier(bpIdentifier))
			{
				bpIdentifier = Guid.NewGuid().ToString() + "|" + DateTime.Now.ToString();
				Environment.SetEnvironmentVariable("__l10n_build", bpIdentifier, EnvironmentVariableTarget.Machine);
			}

			context.Diagnostics.Add(
						Diagnostic.Create(
							new DiagnosticDescriptor("L10n", "TEST", "BuildIdentifier: " + bpIdentifier, "Translated", DiagnosticSeverity.Info, true),
							Location.None));

			var baseDir = new DirectoryInfo(context.Arguments.BaseDirectory);

			while (!baseDir.GetFiles("*.sln").Any())
			{
				baseDir = baseDir.Parent;
			}

			var solutionDir = baseDir.FullName;

			PhraseInstance = L10n.CreateInstance(new NullLanguageProvider("1"), new FileDataProvider(solutionDir));

			var methods = new[] 
			{
				"_s",
				"_m",
				"MN.L10n.L10n._s",
				"MN.L10n.L10n._m"
			};
			
			var phraseRewriter = new PhrasesRewriter("L10n_rw", "MN.L10n.L10n.GetLanguage()", PhraseInstance, bpIdentifier, methods);

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
