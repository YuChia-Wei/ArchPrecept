using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchPrecept.Analyzers.Core;

/// <summary>
/// 驗證聚合儲存庫、查詢儲存庫與相容性儲存庫契約的語意邊界。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RepositoryQueryMethodAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> AllowedAggregateMethods =
        ImmutableHashSet.Create(StringComparer.Ordinal, "FindByIdAsync", "SaveAsync");

    private static readonly string[] WriteMethodPrefixes =
    {
        "Save",
        "Add",
        "Create",
        "Insert",
        "Update",
        "Delete",
        "Remove",
        "Upsert",
        "Persist",
        "Purge"
    };

    /// <summary>
    /// 取得此分析器支援的 Diagnostic 描述項目。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(RuleDescriptors.RepositoryQueryMethod);

    /// <summary>
    /// 初始化分析器並註冊具名型別的符號分析。
    /// </summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    /// <summary>
    /// 分析介面型別是否為受限制的通用寫入儲存庫、聚合儲存庫或查詢儲存庫。
    /// </summary>
    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind is not (TypeKind.Interface or TypeKind.Class or TypeKind.Struct))
        {
            return;
        }

        if (type.TypeKind == TypeKind.Interface && RoleClassifier.For(context.Compilation, context.Options).IsDirectContract(type, "generic-repository"))
        {
            Report(context, type, $"public generic writable repository '{type.Name}' is prohibited");
            return;
        }

        var aggregateContracts = GetAggregateContracts(type, RoleClassifier.For(context.Compilation, context.Options));
        if (!aggregateContracts.IsEmpty)
        {
            AnalyzeAggregateRepository(context, type, aggregateContracts);
        }

        if (RoleClassifier.For(context.Compilation, context.Options).Is(type, "query-repository"))
        {
            AnalyzeQueryRepository(context, type);
        }
    }

    /// <summary>
    /// 檢查聚合儲存庫的相容性繼承、聚合根型別與允許的方法簽章。
    /// </summary>
    private static void AnalyzeAggregateRepository(
        SymbolAnalysisContext context,
        INamedTypeSymbol type,
        ImmutableArray<INamedTypeSymbol> aggregateContracts)
    {
        if (RoleClassifier.For(context.Compilation, context.Options).IsDirectContract(type, "compatibility-repository")
            && !type.AllInterfaces.Any(candidate => RoleClassifier.For(context.Compilation, context.Options).IsDirectContract(candidate, "aggregate-repository")))
        {
            Report(
                context,
                type,
                "IDomainRepository is a compatibility contract and must inherit IAggregateRepository");
        }

        var invalidAggregate = aggregateContracts
            .Select(contract => contract.TypeArguments[0])
            .FirstOrDefault(candidate => !RoleClassifier.For(context.Compilation, context.Options).Is(candidate, "aggregate"));

        if (invalidAggregate is not null)
        {
            Report(
                context,
                type,
                $"aggregate repository type '{invalidAggregate.ToDisplayString()}' is not an Aggregate Root");
        }

        foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
        {
            if (method.MethodKind != MethodKind.Ordinary)
            {
                continue;
            }

            if (!AllowedAggregateMethods.Contains(method.Name))
            {
                Report(
                    context,
                    method,
                    $"aggregate repository member '{method.Name}' is outside the FindByIdAsync/SaveAsync contract");
                continue;
            }

            var contract = aggregateContracts.First();
            if (!HasApprovedAggregateMethodSignature(
                    method,
                    contract.TypeArguments[0],
                    contract.TypeArguments[1]))
            {
                Report(
                    context,
                    method,
                    $"aggregate repository member '{method.Name}' does not match the approved asynchronous signature");
            }
        }
    }

    /// <summary>
    /// 檢查查詢儲存庫不暴露寫入方法或可變動的領域物件回傳型別。
    /// </summary>
    private static void AnalyzeQueryRepository(
        SymbolAnalysisContext context,
        INamedTypeSymbol type)
    {
        foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
        {
            if (method.MethodKind != MethodKind.Ordinary)
            {
                continue;
            }

            if (LooksLikeWriteMethod(method.Name))
            {
                Report(
                    context,
                    method,
                    $"query repository member '{method.Name}' exposes a persistence write");
                continue;
            }

            if (ContainsMutableDomainType(method.ReturnType, RoleClassifier.For(context.Compilation, context.Options), new System.Collections.Generic.HashSet<ITypeSymbol>(SymbolEqualityComparer.Default)))
            {
                Report(
                    context,
                    method,
                    $"query repository member '{method.Name}' returns an Aggregate Root or Domain Entity");
            }
        }
    }

    /// <summary>收集中央分類器辨識的兩參數聚合 repository 契約，保留實際泛型引數。</summary>
    private static ImmutableArray<INamedTypeSymbol> GetAggregateContracts(INamedTypeSymbol type, RoleClassifier classifier) =>
        RoleClassifier.TypeClosure(type).OfType<INamedTypeSymbol>()
            .Where(candidate => candidate.TypeArguments.Length == 2
                && (classifier.IsDirectContract(candidate, "aggregate-repository")
                    || classifier.IsDirectContract(candidate, "compatibility-repository")))
            .ToImmutableArray();

    /// <summary>
    /// 判斷聚合儲存庫方法是否符合 <c>FindByIdAsync</c> 或 <c>SaveAsync</c> 的核准非同步簽章。
    /// </summary>
    private static bool HasApprovedAggregateMethodSignature(
        IMethodSymbol method,
        ITypeSymbol aggregateType,
        ITypeSymbol idType)
    {
        if (method.Parameters.Length < 1 || method.Parameters.Length > 2)
        {
            return false;
        }

        if (method.Parameters.Length == 2
            && !IsCancellationToken(method.Parameters[1].Type))
        {
            return false;
        }

        if (method.Name == "FindByIdAsync")
        {
            return SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, idType)
                && IsTaskOf(method.ReturnType, aggregateType);
        }

        return method.Name == "SaveAsync"
            && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, aggregateType)
            && IsTask(method.ReturnType);
    }

    /// <summary>
    /// 判斷型別是否為承載指定結果型別的 <c>Task&lt;T&gt;</c>。
    /// </summary>
    private static bool IsTaskOf(ITypeSymbol type, ITypeSymbol resultType) =>
        type is INamedTypeSymbol namedType
        && namedType.Name == "Task"
        && namedType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks"
        && namedType.TypeArguments.Length == 1
        && SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], resultType);

    /// <summary>
    /// 判斷型別是否為不承載結果的 <c>Task</c>。
    /// </summary>
    private static bool IsTask(ITypeSymbol type) =>
        type is INamedTypeSymbol namedType
        && namedType.Name == "Task"
        && namedType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks"
        && namedType.TypeArguments.Length == 0;

    /// <summary>
    /// 判斷型別是否為 <c>System.Threading.CancellationToken</c>。
    /// </summary>
    private static bool IsCancellationToken(ITypeSymbol type) =>
        type.Name == "CancellationToken"
        && type.ContainingNamespace.ToDisplayString() == "System.Threading";

    /// <summary>在有界的宣告回傳型別內檢查 Entity／Aggregate 與陣列、泛型引數。</summary>
    private static bool ContainsMutableDomainType(ITypeSymbol type, RoleClassifier classifier,
        System.Collections.Generic.HashSet<ITypeSymbol> visited)
    {
        if (!visited.Add(type)) return false;
        if (classifier.Is(type, "aggregate") || classifier.Is(type, "entity")) return true;
        if (type is IArrayTypeSymbol array) return ContainsMutableDomainType(array.ElementType, classifier, visited);
        return type is INamedTypeSymbol named
            && named.TypeArguments.Any(argument => ContainsMutableDomainType(argument, classifier, visited));
    }

    /// <summary>
    /// 判斷方法名稱是否以已知的持久化寫入動詞開頭。
    /// </summary>
    private static bool LooksLikeWriteMethod(string methodName) =>
        WriteMethodPrefixes.Any(prefix =>
            methodName.StartsWith(prefix, StringComparison.Ordinal));

    /// <summary>
    /// 在符號的來源位置回報儲存庫契約違規 Diagnostic。
    /// </summary>
    private static void Report(
        SymbolAnalysisContext context,
        ISymbol symbol,
        string reason)
    {
        var location = symbol.Locations.FirstOrDefault(candidate => candidate.IsInSource);
        if (location is null)
        {
            return;
        }

        DiagnosticReporting.Report(context,
            RuleDescriptors.RepositoryQueryMethod,
            location,
            reason);
    }
}
