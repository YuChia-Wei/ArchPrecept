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
    private static readonly string[] ForbiddenTypeSuffixes = { ".DbContext", "DbContext" };

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
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    /// <summary>
    /// 分析聚合根或領域實體的類別宣告，並回報對基礎設施型別的參考。
    /// </summary>
    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);

        if (classSymbol is null || !LooksLikeAggregateOrEntity(classSymbol))
        {
            return;
        }

        foreach (var typeSyntax in classDeclaration.DescendantNodes().OfType<TypeSyntax>())
        {
            var type = context.SemanticModel.GetTypeInfo(typeSyntax, context.CancellationToken).Type;
            if (type is null || !IsForbiddenInfrastructureType(type))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                RuleDescriptors.AggregateInfrastructureDependency,
                typeSyntax.GetLocation(),
                classSymbol.Name,
                type.Name));
        }
    }

    /// <summary>
    /// 判斷型別是否依名稱或繼承關係呈現聚合根或領域實體角色。
    /// </summary>
    private static bool LooksLikeAggregateOrEntity(INamedTypeSymbol symbol)
    {
        if (symbol.Name.EndsWith("Aggregate", System.StringComparison.Ordinal)
            || symbol.Name.EndsWith("AggregateRoot", System.StringComparison.Ordinal)
            || symbol.Name.EndsWith("Entity", System.StringComparison.Ordinal))
        {
            return true;
        }

        for (var current = symbol.BaseType; current is not null; current = current.BaseType)
        {
            var name = current.Name;
            if (name == "AggregateRoot" || name == "Entity" || name.EndsWith("AggregateRoot", System.StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 判斷型別完整名稱是否符合受禁止的基礎設施型別尾碼。
    /// </summary>
    private static bool IsForbiddenInfrastructureType(ITypeSymbol type)
    {
        var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        foreach (var suffix in ForbiddenTypeSuffixes)
        {
            if (fullName.EndsWith(suffix, System.StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
