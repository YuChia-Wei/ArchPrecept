using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EngineeringGuardrails.Analyzers.Core;

/// <summary>
/// 驗證聚合儲存庫、查詢儲存庫與相容性儲存庫契約的語意邊界。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RepositoryQueryMethodAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> AllowedAggregateMethods =
        ImmutableHashSet.Create(StringComparer.Ordinal, "FindByIdAsync", "SaveAsync");

    private static readonly ImmutableHashSet<string> ForbiddenGenericWriteRepositories =
        ImmutableHashSet.Create(
            StringComparer.Ordinal,
            "IRepository",
            "IGenericRepository",
            "IWritableRepository",
            "ICrudRepository");

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
        if (type.TypeKind != TypeKind.Interface)
        {
            return;
        }

        if (IsForbiddenGenericWriteRepository(type))
        {
            Report(context, type, $"public generic writable repository '{type.Name}' is prohibited");
            return;
        }

        var aggregateContracts = GetAggregateContracts(type);
        if (!aggregateContracts.IsEmpty)
        {
            AnalyzeAggregateRepository(context, type, aggregateContracts);
        }

        if (IsOrImplements(type, "IQueryRepository", 0))
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
        if (IsContract(type, "IDomainRepository", 2)
            && !type.AllInterfaces.Any(candidate => IsContract(candidate, "IAggregateRepository", 2)))
        {
            Report(
                context,
                type,
                "IDomainRepository is a compatibility contract and must inherit IAggregateRepository");
        }

        var invalidAggregate = aggregateContracts
            .Select(contract => contract.TypeArguments[0])
            .FirstOrDefault(candidate => !IsAggregateRoot(candidate));

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

            if (ContainsMutableDomainType(method.ReturnType))
            {
                Report(
                    context,
                    method,
                    $"query repository member '{method.Name}' returns an Aggregate Root or Domain Entity");
            }
        }
    }

    /// <summary>
    /// 收集型別本身及其繼承介面中的聚合儲存庫契約。
    /// </summary>
    private static ImmutableArray<INamedTypeSymbol> GetAggregateContracts(INamedTypeSymbol type)
    {
        var builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();

        if (IsAggregateContract(type))
        {
            builder.Add(type);
        }

        foreach (var candidate in type.AllInterfaces)
        {
            if (IsAggregateContract(candidate))
            {
                builder.Add(candidate);
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// 判斷型別是否為標準或相容性的聚合儲存庫契約。
    /// </summary>
    private static bool IsAggregateContract(INamedTypeSymbol type) =>
        IsContract(type, "IAggregateRepository", 2)
        || IsContract(type, "IDomainRepository", 2);

    /// <summary>
    /// 判斷型別本身或其繼承介面是否符合指定名稱與泛型參數數量的契約。
    /// </summary>
    private static bool IsOrImplements(INamedTypeSymbol type, string name, int arity) =>
        IsContract(type, name, arity)
        || type.AllInterfaces.Any(candidate => IsContract(candidate, name, arity));

    /// <summary>
    /// 判斷型別的原始定義是否符合指定名稱與泛型參數數量。
    /// </summary>
    private static bool IsContract(INamedTypeSymbol type, string name, int arity) =>
        type.OriginalDefinition.Name == name
        && type.OriginalDefinition.Arity == arity;

    /// <summary>
    /// 判斷型別是否為公開受禁止的通用可寫入儲存庫契約。
    /// </summary>
    private static bool IsForbiddenGenericWriteRepository(INamedTypeSymbol type) =>
        type.Arity > 0
        && ForbiddenGenericWriteRepositories.Contains(type.OriginalDefinition.Name);

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

    /// <summary>
    /// 判斷型別或其限制條件、繼承階層是否表示聚合根。
    /// </summary>
    private static bool IsAggregateRoot(ITypeSymbol type)
    {
        if (type is ITypeParameterSymbol typeParameter)
        {
            return typeParameter.ConstraintTypes.Any(IsAggregateRoot);
        }

        if (type is not INamedTypeSymbol namedType)
        {
            return false;
        }

        for (var current = namedType; current is not null; current = current.BaseType)
        {
            if (current.Name == "AggregateRoot" || current.Name == "EsAggregateRoot")
            {
                return true;
            }
        }

        return namedType.Name == "IAggregateRoot"
            || namedType.AllInterfaces.Any(candidate => candidate.Name == "IAggregateRoot");
    }

    /// <summary>
    /// 判斷型別或其泛型引數是否包含聚合根或領域實體等可變領域型別。
    /// </summary>
    private static bool ContainsMutableDomainType(ITypeSymbol type)
    {
        if (IsAggregateRoot(type) || IsDomainEntity(type))
        {
            return true;
        }

        return type is INamedTypeSymbol namedType
            && namedType.TypeArguments.Any(ContainsMutableDomainType);
    }

    /// <summary>
    /// 判斷型別或其限制條件、繼承階層是否表示領域實體。
    /// </summary>
    private static bool IsDomainEntity(ITypeSymbol type)
    {
        if (type is ITypeParameterSymbol typeParameter)
        {
            return typeParameter.ConstraintTypes.Any(IsDomainEntity);
        }

        if (type is not INamedTypeSymbol namedType)
        {
            return false;
        }

        for (var current = namedType; current is not null; current = current.BaseType)
        {
            if (current.Name == "DomainEntity")
            {
                return true;
            }
        }

        return namedType.Name == "IDomainEntity"
            || namedType.AllInterfaces.Any(candidate => candidate.Name == "IDomainEntity");
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

        context.ReportDiagnostic(Diagnostic.Create(
            RuleDescriptors.RepositoryQueryMethod,
            location,
            reason));
    }
}
