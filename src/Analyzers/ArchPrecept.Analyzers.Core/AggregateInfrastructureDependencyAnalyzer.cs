using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchPrecept.Analyzers.Core;

/// <summary>
/// 分析聚合根與領域實體是否直接參考基礎設施型別。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AggregateInfrastructureDependencyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// 取得此分析器支援的 Diagnostic 描述項目。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(RuleDescriptors.AggregateInfrastructureDependency);

    /// <summary>
    /// 初始化分析器並註冊類別宣告的語法節點分析。
    /// </summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration, SyntaxKind.RecordDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.RecordStructDeclaration);
    }

    /// <summary>
    /// 分析聚合根或領域實體的類別宣告，並回報對基礎設施型別的參考。
    /// </summary>
    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (TypeDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);

        if (classSymbol is null || !(RoleClassifier.For(context.Compilation, context.Options).Is(classSymbol, "aggregate") || RoleClassifier.For(context.Compilation, context.Options).Is(classSymbol, "entity")))
        {
            return;
        }

        foreach (var typeSyntax in classDeclaration.DescendantNodes().OfType<TypeSyntax>())
        {
            var type = context.SemanticModel.GetTypeInfo(typeSyntax, context.CancellationToken).Type;
            if (type is null || !RoleClassifier.For(context.Compilation, context.Options).Is(type, "db-context"))
            {
                continue;
            }

            DiagnosticReporting.Report(context,
                RuleDescriptors.AggregateInfrastructureDependency,
                typeSyntax.GetLocation(),
                classSymbol.Name,
                type.Name);
        }
    }

}
