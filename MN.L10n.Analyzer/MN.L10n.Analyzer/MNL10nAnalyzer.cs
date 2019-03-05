using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MN.L10n.Analyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class MNL10nAnalyzer : DiagnosticAnalyzer
	{
        public static readonly DiagnosticDescriptor NoParamRule = new DiagnosticDescriptor("MN0001", "Missing arguments", "Need to send variables to '{0}'", "L10n", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor MemberAccessorRule = new DiagnosticDescriptor("MN0002", "Input is not a known string", "L10n can only evaluate string literals when finding used phrases. If the phrase is known you can ignore this, but you should only use L10n with known strings when possible.", "L10n", DiagnosticSeverity.Warning, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor NoWhitespaceAtStartOrEndRule = new DiagnosticDescriptor("MN0003", "String starts/ends with whitespace", "The string cannot start or end with whitespaces.", "L10n", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor NoEmptyStringsEndRule = new DiagnosticDescriptor("MN0004", "Input is an empty string", "The string cannot start or end with whitespace", "L10n", DiagnosticSeverity.Error, isEnabledByDefault: true);
        public static readonly DiagnosticDescriptor NoStringInterpolationRule = new DiagnosticDescriptor("MN0005", "Interpolated string used", "L10n can only evaluate string literals when finding used phrases. Never use string interpolation with L10n. It is not supported yet.", "L10n", DiagnosticSeverity.Error, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(NoParamRule, MemberAccessorRule, NoWhitespaceAtStartOrEndRule, NoEmptyStringsEndRule, NoStringInterpolationRule); } }

		public override void Initialize(AnalysisContext context)
		{
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
		}

        private static string[] validIdentifiers = new[]
        {
            "_s", "_sr", "_m", "_mr"
        };

		private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext obj)
		{
            var ies = obj.Node as InvocationExpressionSyntax;
            if (ies == null) return;
            
            var expr = ies.Expression as IdentifierNameSyntax;
            if (expr == null || !validIdentifiers.Contains(expr.Identifier.Text)) return;
                
            var arguments = ies.ArgumentList.Arguments;
            if(arguments.Count == 0)
                obj.ReportDiagnostic(Diagnostic.Create(NoParamRule, obj.Node.GetLocation()));
            else
            {
                var supposedString = arguments.First();
                switch(supposedString.Expression)
                {
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
                            if(char.IsWhiteSpace(text.First()) || char.IsWhiteSpace(text.Last()))
                            {
                                obj.ReportDiagnostic(Diagnostic.Create(NoWhitespaceAtStartOrEndRule, obj.Node.GetLocation()));
                            }
                        }
                        break;
                }
            }
		}
	}
}
