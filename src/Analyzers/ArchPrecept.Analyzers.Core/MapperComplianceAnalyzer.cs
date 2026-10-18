using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchPrecept.Analyzers.Core;

/// <summary>
/// 驗證 Mapper 類別的靜態性與相依性是否維持純轉換邊界。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MapperComplianceAnalyzer : DiagnosticAnalyzer
{
    private static readonly string[] ForbiddenTypeSuffixes =
    {
        "Repository",
        "UseCase",
        "Handler"
    };

    /// <summary>
    /// 取得此分析器支援的 Diagnostic 描述項目。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            RuleDescriptors.MapperMustBeStatic,
            RuleDescriptors.MapperForbiddenDependency);

    /// <summary>
    /// 初始化分析器並註冊類別宣告的語法節點分析。
    /// </summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMapper, SyntaxKind.ClassDeclaration);
    }

    /// <summary>
    /// 分析名稱以 <c>Mapper</c> 結尾的類別是否為靜態且未參考受禁止相依性。
    /// </summary>
    private static void AnalyzeMapper(SyntaxNodeAnalysisContext context)
    {
        var declaration = (ClassDeclarationSyntax)context.Node;
        var mapperSymbol = context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken);
        if (mapperSymbol is null || !mapperSymbol.Name.EndsWith("Mapper", StringComparison.Ordinal))
        {
            return;
        }

        if (!mapperSymbol.IsStatic)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                RuleDescriptors.MapperMustBeStatic,
                declaration.Identifier.GetLocation(),
                mapperSymbol.Name));
        }

        foreach (var typeSyntax in declaration.DescendantNodes().OfType<TypeSyntax>())
        {
            var type = context.SemanticModel.GetTypeInfo(typeSyntax, context.CancellationToken).Type;
            if (type is null || !IsForbiddenDependency(type))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                RuleDescriptors.MapperForbiddenDependency,
                typeSyntax.GetLocation(),
                mapperSymbol.Name,
                type.Name));
        }
    }

    /// <summary>
    /// 判斷型別是否為 Repository、Use Case 或 Handler 等 Mapper 不應依賴的型別。
    /// </summary>
    private static bool IsForbiddenDependency(ITypeSymbol type)
    {
        var candidate = type is INamedTypeSymbol { IsGenericType: true } namedType
            ? namedType.ConstructedFrom.Name
            : type.Name;

        foreach (var suffix in ForbiddenTypeSuffixes)
        {
            if (candidate.EndsWith(suffix, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
