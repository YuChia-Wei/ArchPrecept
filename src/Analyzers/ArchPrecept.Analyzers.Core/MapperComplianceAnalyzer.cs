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
    /// <summary>建立分析器實例；規則與角色設定由每次編譯提供，不保存 consumer 執行期狀態。</summary>
    public MapperComplianceAnalyzer() { }

    /// <summary>
    /// 取得此分析器支援的 Diagnostic 描述項目。
    /// </summary>
    /// <value>此分析器可回報的描述項；是否啟用及有效嚴重性由本次設定決定。</value>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            RuleDescriptors.MapperMustBeStatic,
            RuleDescriptors.MapperForbiddenDependency);

    /// <summary>
    /// 初始化分析器並註冊類別宣告的語法節點分析。
    /// </summary>
    /// <param name="context">Roslyn 提供的註冊環境；由編譯器管理分析與取消生命週期。</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMapper, SyntaxKind.ClassDeclaration, SyntaxKind.RecordDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.RecordStructDeclaration);
    }

    /// <summary>
    /// 分析已辨識 Mapper 是否為靜態且未參考受禁止相依性。
    /// </summary>
    private static void AnalyzeMapper(SyntaxNodeAnalysisContext context)
    {
        var declaration = (TypeDeclarationSyntax)context.Node;
        var mapperSymbol = context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken);
        if (mapperSymbol is null || !RoleClassifier.For(context.Compilation, context.Options).Is(mapperSymbol, "mapper"))
        {
            return;
        }

        if (!mapperSymbol.IsStatic)
        {
            DiagnosticReporting.Report(context,
                RuleDescriptors.MapperMustBeStatic,
                declaration.Identifier.GetLocation(),
                mapperSymbol.Name);
        }

        foreach (var typeSyntax in declaration.DescendantNodes(node => node == declaration || node is not TypeDeclarationSyntax).OfType<TypeSyntax>())
        {
            var type = context.SemanticModel.GetTypeInfo(typeSyntax, context.CancellationToken).Type;
            if (type is null || !(RoleClassifier.For(context.Compilation, context.Options).Is(type, "repository") || RoleClassifier.For(context.Compilation, context.Options).Is(type, "use-case") || RoleClassifier.For(context.Compilation, context.Options).Is(type, "handler")))
            {
                continue;
            }

            DiagnosticReporting.Report(context,
                RuleDescriptors.MapperForbiddenDependency,
                typeSyntax.GetLocation(),
                mapperSymbol.Name,
                type.Name);
        }
    }

}
