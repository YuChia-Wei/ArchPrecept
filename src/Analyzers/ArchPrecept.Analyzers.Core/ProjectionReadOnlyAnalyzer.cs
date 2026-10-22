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
    private static readonly ImmutableHashSet<string> WriteMethods =
        ImmutableHashSet.Create(StringComparer.Ordinal, "Add", "AddAsync", "Update", "Remove");

    /// <summary>
    /// 取得此分析器支援的 Diagnostic 描述項目。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(RuleDescriptors.ProjectionWriteOperation);

    /// <summary>
    /// 初始化分析器並註冊方法呼叫運算式的語法節點分析。
    /// </summary>
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
        if (method is null || !IsPersistenceWrite(method, RoleClassifier.For(context.Compilation, context.Options)))
        {
            return;
        }

        DiagnosticReporting.Report(context,
            RuleDescriptors.ProjectionWriteOperation,
            invocation.GetLocation(),
            projectionType.Name,
            method.Name);
    }

    /// <summary>
    /// 判斷方法是否為 <c>DbContext</c> 或 <c>DbSet</c> 的持久化寫入作業。
    /// </summary>
    private static bool IsPersistenceWrite(IMethodSymbol method, RoleClassifier classifier)
    {
        if (method.Name is "SaveChanges" or "SaveChangesAsync")
        {
            return classifier.Is(method.ContainingType, "db-context");
        }

        return WriteMethods.Contains(method.Name)
            && (classifier.Is(method.ContainingType, "db-context")
                || classifier.Is(method.ContainingType, "db-set"));
    }

}
