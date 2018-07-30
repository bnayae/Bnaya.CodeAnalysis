using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Bnaya.CodeAnalysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BnayaEqualityAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "BnayaEqualityAnalyzer";
        private const string Category = "Correctness";

        #region DiagnosticDescriptor RuleForStruct = 

        private static DiagnosticDescriptor RuleForStruct = new DiagnosticDescriptor(
                    DiagnosticId,
                    "Equality Pattern",
                    "Type name '{0}' don't implement Equality pattern",
                    Category, DiagnosticSeverity.Warning,
                    isEnabledByDefault: true,
                    description: "Don't implement Equality pattern");

        #endregion // DiagnosticDescriptor RuleForStruct = 

        #region ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(RuleForStruct);
            }
        }

        #endregion // ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics

        #region Initialize

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        #endregion // Initialize

        //private static void AnalyzeSymbol(SymbolAnalysisContext context)
        //{
        //    // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
        //    var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        //    // Find just those named type symbols with names containing lowercase letters.
        //    if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
        //    {
        //        // For all such symbols, produce a diagnostic.
        //        var diagnostic = Diagnostic.Create(RuleForStruct, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

        //        context.ReportDiagnostic(diagnostic);
        //    }
        //}

        #region AnalyzeSymbol

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, 
            //       generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;


            bool isClass = namedTypeSymbol.TypeKind == TypeKind.Class;
            bool isStruct = namedTypeSymbol.TypeKind == TypeKind.Struct;

            if (isClass || isStruct)
            {
                //bool hasAutoEquatable = context.Symbol.GetAttributes()
                //                .Any(att => att.AttributeClass.Name == "AutoEquatable");
                var implementEquatable = namedTypeSymbol.AllInterfaces
                                            .Any(m => m.Name == "IEquatable");

                if (isStruct &&
                    !implementEquatable)
                {
                    var diagnostic = Diagnostic.Create(RuleForStruct, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                    context.ReportDiagnostic(diagnostic);
                }

                //if (hasAutoEquatable && isClass &&
                //    !implementEquatable)
                //{
                //    var diagnostic = Diagnostic.Create(RuleByAttribute, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                //    context.ReportDiagnostic(diagnostic);
                //}
            }
        }

        #endregion // AnalyzeSymbol
    }
}
