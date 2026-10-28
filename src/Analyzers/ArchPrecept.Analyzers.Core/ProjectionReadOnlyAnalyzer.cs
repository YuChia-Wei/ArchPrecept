using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchPrecept.Analyzers.Core;

/// <summary>
/// 驗證 Projection 類別不會呼叫 EF Core 的持久化寫入作業。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProjectionReadOnlyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>建立分析器實例；規則與角色設定由每次編譯提供，不保存 consumer 執行期狀態。</summary>
    public ProjectionReadOnlyAnalyzer() { }

    /// <summary>
    /// 取得此分析器支援的 Diagnostic 描述項目。
    /// </summary>
    /// <value>此分析器可回報的描述項；是否啟用及有效嚴重性由本次設定決定。</value>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(RuleDescriptors.ProjectionWriteOperation);

    /// <summary>
    /// 初始化分析器並註冊方法呼叫運算式的語法節點分析。
    /// </summary>
    /// <param name="context">Roslyn 提供的註冊環境；由編譯器管理分析與取消生命週期。</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    /// <summary>
    /// 檢查 Projection 內的方法呼叫是否為持久化寫入作業。
    /// </summary>
    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var projectionType = context.ContainingSymbol?.ContainingType;
        if (projectionType is null || !RoleClassifier.For(context.Compilation, context.Options).Is(projectionType, "projection"))
        {
            return;
        }

        var method = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;
        if (method is null || !PersistenceWriteFacts.IsWrite(method, RoleClassifier.For(context.Compilation, context.Options)))
        {
            return;
        }

        DiagnosticReporting.Report(context,
            RuleDescriptors.ProjectionWriteOperation,
            invocation.GetLocation(),
            projectionType.Name,
            method.Name);
    }

}
