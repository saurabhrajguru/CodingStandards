using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace IntelliTectAnalyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnusedLocalVariable : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "INTL0004";
        private const string Title = "Unused Local Variable";
        private const string MessageFormat = "Unused local variables should be removed";
        private const string Description = "If a local variable is declared but not used, it is dead code and should be removed.";
        private const string Category = "Performance";
        private const string HelpLinkUri = "https://github.com/IntelliTect/CodingStandards";

        private static readonly DiagnosticDescriptor _Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
           Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_Rule);

        public override void Initialize(AnalysisContext context)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            context.RegisterSyntaxNodeAction(AnalyzeUnUnsedLocal, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeUnUnsedLocal(SyntaxNodeAnalysisContext context)
        {
            var method = context.Node as MethodDeclarationSyntax;

            var dataFlow = context.SemanticModel.AnalyzeDataFlow(method.Body);

            var variablesDeclared = dataFlow.VariablesDeclared;
            var variablesRead = dataFlow.ReadInside.Union(dataFlow.ReadOutside);
            var unused = variablesDeclared.Except(variablesRead);

            if (unused.Any())
            {
                foreach (var unusedVar in unused)
                {
                    context.ReportDiagnostic(Diagnostic.Create(_Rule, unusedVar.Locations.First()));
                }
            }
        }

    }
}
