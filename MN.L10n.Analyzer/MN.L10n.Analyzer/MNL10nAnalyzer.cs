using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace MN.L10n.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MNL10nAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor NoParamRule = new DiagnosticDescriptor("MN0001", "Missing arguments", "Need to send variables to '{0}'", "L10n", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor MemberAccessorRule = new DiagnosticDescriptor("MN0002", "Input is not a known string", "L10n can only evaluate string literals when finding used phrases. If the phrase is known you can ignore this, but you should only use L10n with known strings when possible.", "L10n", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor NoWhitespaceAtStartOrEndRule = new DiagnosticDescriptor("MN0003", "String starts/ends with whitespace", "The string cannot start or end with whitespaces", "L10n", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor NoEmptyStringsEndRule = new DiagnosticDescriptor("MN0004", "Input is an empty string", "The string cannot start or end with whitespace", "L10n", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor NoStringInterpolationRule = new DiagnosticDescriptor("MN0005", "Interpolated string used", "L10n can only evaluate string literals when finding used phrases. Never use string interpolation with L10n. It is not supported yet.", "L10n", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor NoStringConcatRule = new DiagnosticDescriptor("MN0006", "String concatenation used", "L10n can only evaluate a single string literal when finding used phrases. Never use string concatenation with L10n. It is not supported yet.", "L10n", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor ArgumentsNotAnClassOrNullRule = new DiagnosticDescriptor("MN0007", "Invalid type for keywords", "L10n requires a class or anonymous type (or explicitly null) for keywords", "L10n", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor MissingKeywordReplacementObjectRule = new DiagnosticDescriptor("MN0008", "Missing object for keyword replacement", "L10n requires a class or anonymous type (or explicitly null) for keywords", "L10n", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor MissingKeywordsInReplacementObjectRule = new DiagnosticDescriptor("MN0009", "Missing property for keyword replacement", "L10n is missing '{0}' in the object for keywords", "L10n", DiagnosticSeverity.Error, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    NoParamRule,
                    MemberAccessorRule,
                    NoWhitespaceAtStartOrEndRule,
                    NoEmptyStringsEndRule,
                    NoStringInterpolationRule,
                    NoStringConcatRule,
                    ArgumentsNotAnClassOrNullRule,
                    MissingKeywordReplacementObjectRule,
                    MissingKeywordsInReplacementObjectRule
                    );
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
        }

        private static string[] validIdentifiers = new[]
        {
            "_s", "_sr", "_m", "_mr",
            "L10n._s", "L10n._sr", "L10n._m", "L10n._mr",
        };

        static Regex r = new Regex(@"(\$(?:[a-zA-Z0-9_]+?)\$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static HashSet<string> L10nParameters(string input) => new HashSet<string>(r.Matches(input).Cast<Match>().Select(m => m.Groups[1].Value));

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext obj)
        {
            var ies = obj.Node as InvocationExpressionSyntax;
            if (ies == null) return;

            var identifier = (ies.Expression as IdentifierNameSyntax)?.Identifier.Text
                ?? (ies.Expression as MemberAccessExpressionSyntax)?.ToString();

            if (!validIdentifiers.Contains(identifier)) return;

            HashSet<string> l10nParameters = null;

            var arguments = ies.ArgumentList.Arguments;
            if (arguments.Count == 0)
                obj.ReportDiagnostic(Diagnostic.Create(NoParamRule, obj.Node.GetLocation()));
            else
            {
                var supposedString = arguments.First();
                switch (supposedString.Expression)
                {
                    case BinaryExpressionSyntax binaryExpression:
                        obj.ReportDiagnostic(Diagnostic.Create(NoStringConcatRule, obj.Node.GetLocation()));
                        break;
                    case MemberAccessExpressionSyntax memberAccess:
                    case InvocationExpressionSyntax invocationExpression:
                        obj.ReportDiagnostic(Diagnostic.Create(MemberAccessorRule, obj.Node.GetLocation()));
                        break;
                    case InterpolatedStringExpressionSyntax interpolatedString:
                        obj.ReportDiagnostic(Diagnostic.Create(NoStringInterpolationRule, obj.Node.GetLocation()));
                        break;
                    case LiteralExpressionSyntax literalExpression:
                        var text = literalExpression.Token.ValueText;
                        if (string.IsNullOrWhiteSpace(text))
                        {
                            obj.ReportDiagnostic(Diagnostic.Create(NoEmptyStringsEndRule, obj.Node.GetLocation()));
                        }
                        else
                        {
                            if (char.IsWhiteSpace(text.First()) || char.IsWhiteSpace(text.Last()))
                            {
                                obj.ReportDiagnostic(Diagnostic.Create(NoWhitespaceAtStartOrEndRule, obj.Node.GetLocation()));
                            }
                        }

                        if (text.Contains("$"))
                        {
                            l10nParameters = L10nParameters(text);

                            if (arguments.Count == 1)
                            {
                                obj.ReportDiagnostic(Diagnostic.Create(MissingKeywordReplacementObjectRule, obj.Node.GetLocation()));
                            }
                        }

                        break;
                }

                if (arguments.Count > 1)
                {
                    var supposedArgument = arguments[1];

                    if (!(supposedArgument.Expression is ObjectCreationExpressionSyntax || supposedArgument.Expression is AnonymousObjectCreationExpressionSyntax || supposedArgument.Expression.RawKind == (int)SyntaxKind.NullLiteralExpression))
                    {
                        obj.ReportDiagnostic(Diagnostic.Create(ArgumentsNotAnClassOrNullRule, obj.Node.GetLocation()));
                    }
                    else
                    {
                        if (l10nParameters == null || l10nParameters.Count == 0)
                        {
                            return;
                        }

                        var argumentAsObject = obj.SemanticModel.GetSymbolInfo(supposedArgument.Expression).Symbol;

                        var missingParameters = l10nParameters.Except(argumentAsObject.ContainingType.MemberNames.Select(p => $"${p}$"));

                        if (missingParameters.Any())
                        {
                            foreach (var p in missingParameters)
                            {
                                obj.ReportDiagnostic(Diagnostic.Create(MissingKeywordsInReplacementObjectRule, obj.Node.GetLocation(), p));
                            }
                        }
                    }
                }
            }
        }
    }
}
