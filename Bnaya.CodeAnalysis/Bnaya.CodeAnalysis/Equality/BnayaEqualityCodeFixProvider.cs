using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Bnaya.CodeAnalysis
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BnayaEqualityCodeFixProvider)), Shared]
    public class BnayaEqualityCodeFixProvider : CodeFixProvider
    {
        private const string title = "Implement Equality Pattern";

        #region FixableDiagnosticIds

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(
                                BnayaEqualityAnalyzer.DiagnosticId); }
        }

        #endregion // FixableDiagnosticIds

        #region GetFixAllProvider

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        #endregion // GetFixAllProvider

        #region RegisterCodeFixesAsync

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution: c => FixAsync(context.Document, declaration, c), 
                    equivalenceKey: title),
                diagnostic);
        }

        #endregion // RegisterCodeFixesAsync

        #region FixAsync

        private async Task<Solution> FixAsync(
            Document document,
            TypeDeclarationSyntax typeDecl,
            CancellationToken cancellationToken)
        {
            //// Get the symbol representing the type.
            //var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            //var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            var root = await document.GetSyntaxRootAsync(cancellationToken);

            SyntaxTree attTree = SyntaxFactory.ParseSyntaxTree(@"[System.CodeDom.Compiler.GeneratedCode(""Equality Pattern"", ""V1"")]");
            SyntaxNode attRoot = await attTree.GetRootAsync().ConfigureAwait(false);
            AttributeListSyntax attList = attRoot.DescendantNodes().OfType<AttributeListSyntax>().First();

            var structDecl = typeDecl as StructDeclarationSyntax;
            var newClassNode = structDecl.WithoutLeadingTrivia()
                .AddAttributeLists(attList);

            root = root.ReplaceNode(structDecl, newClassNode).NormalizeWhitespace();
            document = document.WithSyntaxRoot(root);
            return document.Project.Solution;

        }

        #endregion // FixAsync
    }
}
