using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

namespace MN.L10n.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MNL10nCodeFixProvider)), Shared]
    public class MNL10nCodeFixProvider : CodeFixProvider
    {
        private const string title = "Make uppercase";

        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get {
                return ImmutableArray.Create(
              MNL10nAnalyzer.NoParamRule.Id,
              MNL10nAnalyzer.MemberAccessorRule.Id,
              MNL10nAnalyzer.NoEmptyStringsEndRule.Id,
              MNL10nAnalyzer.NoWhitespaceAtStartOrEndRule.Id,
              MNL10nAnalyzer.NoStringInterpolationRule.Id
              );
            }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            await Task.CompletedTask;
        }
    }
}
