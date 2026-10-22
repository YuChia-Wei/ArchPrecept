using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchPrecept.Analyzers.Core;

/// <summary>提供所有 Analyzer 共用、每個 Compilation 快取的角色判定。</summary>
internal sealed class RoleClassifier
{
    private static readonly ConditionalWeakTable<Compilation, ConcurrentDictionary<AnalyzerOptions, RoleClassifier>> States = new();
    private readonly Compilation compilation;
    private readonly ConcurrentDictionary<ISymbol, ConcurrentDictionary<string, RoleMatch>> matches = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<string, INamedTypeSymbol> configured = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ImmutableArray<INamedTypeSymbol>> builtIn = new(StringComparer.Ordinal);
    internal AnalyzerConfiguration Configuration { get; }

    /// <summary>取得同一編譯與同一設定物件共用的分類器。</summary>
    internal static RoleClassifier For(Compilation compilation, AnalyzerOptions options) =>
        States.GetValue(compilation, _ => new ConcurrentDictionary<AnalyzerOptions, RoleClassifier>())
            .GetOrAdd(options, _ => new RoleClassifier(compilation, options));

    /// <summary>解析明確與內建契約，將不存在或有歧義的 mapping 列為設定錯誤。</summary>
    private RoleClassifier(Compilation compilation, AnalyzerOptions options)
    {
        this.compilation = compilation;
        Configuration = AnalyzerConfiguration.Load(options);
        foreach (var mapping in Configuration.Mappings)
        {
            var symbol = Resolve(mapping.Value.MetadataName, mapping.Value.AssemblyName, false);
            if (symbol is null)
                Configuration.Errors.Add($"Role '{mapping.Key}': cannot uniquely resolve {mapping.Value.MetadataName} in assembly {mapping.Value.AssemblyName}.");
            else if (mapping.Key is "aggregate-repository" or "compatibility-repository" && symbol.Arity != 2)
                Configuration.Errors.Add($"Role '{mapping.Key}' requires a contract with aggregate and ID type parameters (arity 2).");
            else
                configured.Add(mapping.Key, symbol);
        }
        AddBuiltIn("aggregate", "ArchPrecept.Contracts.Domain", "IAggregateRoot`1", "AggregateRoot`1");
        AddBuiltIn("entity", "ArchPrecept.Contracts.Domain", "IDomainEntity`1", "DomainEntity`1");
        AddBuiltIn("value-object", "ArchPrecept.Contracts.Domain", "IValueObject", "ValueObject");
        AddBuiltIn("aggregate-repository", "ArchPrecept.Contracts.Application", "IAggregateRepository`2");
        AddBuiltIn("compatibility-repository", "ArchPrecept.Contracts.Application", "IDomainRepository`2");
        AddBuiltIn("query-repository", "ArchPrecept.Contracts.Application", "IQueryRepository");
        AddBuiltIn("domain-event-dispatcher", "ArchPrecept.Contracts.Application", "IDomainEventDispatcher");
        AddBuiltIn("event-sourced-aggregate", "ArchPrecept.Contracts.Domain.EventSourcing", "EsAggregateRoot`1");
        AddPlatform("controller", "Microsoft.AspNetCore.Mvc.ControllerBase", "Microsoft.AspNetCore.Mvc.Core");
        AddPlatform("db-context", "Microsoft.EntityFrameworkCore.DbContext", "Microsoft.EntityFrameworkCore");
        AddPlatform("db-set", "Microsoft.EntityFrameworkCore.DbSet`1", "Microsoft.EntityFrameworkCore");
        AddPlatform("bus", "Wolverine.IMessageBus", "Wolverine");
        AddPlatform("bus", "MediatR.IMediator", "MediatR");
    }

    /// <summary>以 metadata 名稱與組件身分解析唯一符號，不以短名稱放行。</summary>
    private INamedTypeSymbol? Resolve(string metadataName, string assemblyName, bool product)
    {
        var assemblies = compilation.SourceModule.ReferencedAssemblySymbols.Concat(new[] { compilation.Assembly });
        var candidates = assemblies.Where(assembly => assembly.Identity.Name == assemblyName
                && (!product || (assembly.Identity.Version.Major == 0 && string.IsNullOrEmpty(assembly.Identity.CultureName)
                    && assembly.Identity.PublicKeyToken.IsEmpty)))
            .Select(assembly => assembly.GetTypeByMetadataName(metadataName)).Where(symbol => symbol is not null).ToArray();
        return candidates.Length == 1 ? candidates[0] : null;
    }

    /// <summary>登錄內建契約的完整 metadata 與組件名稱。</summary>
    private void AddBuiltIn(string role, string assembly, params string[] names)
    {
        builtIn[role] = names.Select(name => Resolve(assembly + "." + name, assembly, true))
            .Where(symbol => symbol is not null).Cast<INamedTypeSymbol>().ToImmutableArray();
    }

    /// <summary>以完整型別與組件名稱登錄已知平台邊界，未安裝該平台時不新增相依。</summary>
    private void AddPlatform(string role, string metadataName, string assembly)
    {
        var symbol = Resolve(metadataName, assembly, false);
        if (symbol is null) return;
        builtIn.TryGetValue(role, out var current);
        builtIn[role] = (current.IsDefault ? ImmutableArray<INamedTypeSymbol>.Empty : current).Add(symbol);
    }

    /// <summary>取得角色證據；泛型限制與繼承的搜尋具有循環防護。</summary>
    internal RoleMatch Match(ITypeSymbol? type, string role)
    {
        if (type is null || type.TypeKind == TypeKind.Error || Configuration.Errors.Count != 0) return RoleMatch.None;
        return matches.GetOrAdd(type, _ => new ConcurrentDictionary<string, RoleMatch>(StringComparer.Ordinal))
            .GetOrAdd(role, _ => Classify(type, role));
    }

    /// <summary>確認型別符合指定角色。</summary>
    internal bool Is(ITypeSymbol? type, string role) => Match(type, role).Matched;

    /// <summary>依明確 mapping、內建契約、繼承與可選慣例的順序分類。</summary>
    private RoleMatch Classify(ITypeSymbol type, string role)
    {
        var chain = TypeClosure(type).ToArray();
        if (role == "transport" && chain.Any(candidate =>
                (InNamespace(candidate, "Microsoft.AspNetCore") && candidate.ContainingAssembly?.Identity.Name.StartsWith("Microsoft.AspNetCore.", StringComparison.Ordinal) == true)
                || (InNamespace(candidate, "Wolverine") && candidate.ContainingAssembly?.Identity.Name == "Wolverine")
                || (InNamespace(candidate, "MediatR") && candidate.ContainingAssembly?.Identity.Name == "MediatR")))
            return new RoleMatch(role, "built-in-contract", "high", "platform assembly/namespace");
        if (configured.TryGetValue(role, out var contract))
        {
            var match = chain.FirstOrDefault(candidate => SameDefinition(candidate, contract));
            if (match is not null) return new RoleMatch(role, "configured-contract", "high", match.ToDisplayString());
        }
        if (builtIn.TryGetValue(role, out var contracts))
        {
            var match = chain.FirstOrDefault(candidate => contracts.Any(contract => SameDefinition(candidate, contract)));
            if (match is not null)
                return new RoleMatch(role, SameDefinition(type, match) ? "built-in-contract" : "inherited-contract", "high", match.ToDisplayString());
        }
        // Repository is the union of its precise subroles; it has no required runtime marker.
        if (role == "repository")
        {
            foreach (var subrole in new[] { "aggregate-repository", "compatibility-repository", "query-repository", "generic-repository" })
            {
                var match = Classify(type, subrole);
                if (match.Matched) return new RoleMatch(role, match.Evidence, match.Confidence, match.Contract);
            }
        }
        if (role == "entity")
        {
            var aggregate = Classify(type, "aggregate");
            if (aggregate.Matched) return new RoleMatch(role, aggregate.Evidence, aggregate.Confidence, aggregate.Contract);
        }
        if (role == "handler")
        {
            var dispatch = Classify(type, "dispatch-handler");
            if (dispatch.Matched) return new RoleMatch(role, dispatch.Evidence, dispatch.Confidence, dispatch.Contract);
        }
        if (Configuration.Conventions.TryGetValue(role, out var convention))
        {
            if (chain.Any(candidate => ConventionMatches(candidate, convention)))
                return new RoleMatch(role, "naming-convention", "low", "configured suffix/namespace");
            return RoleMatch.None;
        }
        // VO and Entity references in VO rules require explicit evidence even in legacy mode.
        if (Configuration.NamingConventions && chain.Any(candidate => LegacyMatches(candidate, role)))
            return new RoleMatch(role, "naming-convention", "low", "legacy name convention");
        return RoleMatch.None;
    }

    /// <summary>只對候選型別自身判定契約，供需讀取泛型引數的 repository 規則使用。</summary>
    internal bool IsDirectContract(INamedTypeSymbol type, string role)
    {
        if (configured.TryGetValue(role, out var configuredType) && SameDefinition(type, configuredType)) return true;
        if (builtIn.TryGetValue(role, out var contracts) && contracts.Any(contract => SameDefinition(type, contract))) return true;
        return Configuration.NamingConventions && LegacyMatches(type, role);
    }

    /// <summary>比較原始泛型定義的 Roslyn 符號身分。</summary>
    private static bool SameDefinition(ITypeSymbol left, ITypeSymbol right) =>
        SymbolEqualityComparer.Default.Equals(left.OriginalDefinition, right.OriginalDefinition);

    /// <summary>走訪型別、基底、介面與泛型限制，避免自我限制造成循環。</summary>
    internal static IEnumerable<ITypeSymbol> TypeClosure(ITypeSymbol type)
    {
        var pending = new Stack<ITypeSymbol>();
        var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        pending.Push(type);
        while (pending.Count != 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current)) continue;
            yield return current;
            if (current is ITypeParameterSymbol parameter)
                foreach (var constraint in parameter.ConstraintTypes) pending.Push(constraint);
            if (current is INamedTypeSymbol named)
            {
                if (named.BaseType is not null) pending.Push(named.BaseType);
                foreach (var contract in named.Interfaces) pending.Push(contract);
            }
        }
    }

    /// <summary>確認 namespace 完整區段與可選尾碼皆符合。</summary>
    private static bool ConventionMatches(ITypeSymbol type, NamingConvention convention) =>
        (convention.Suffix is null || type.Name.EndsWith(convention.Suffix, StringComparison.Ordinal))
        && (convention.Namespace is null || InNamespace(type, convention.Namespace));

    /// <summary>比對 namespace 本身或其子 namespace，避免相似前綴誤認。</summary>
    internal static bool InNamespace(ITypeSymbol type, string prefix)
    {
        var ns = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        return ns == prefix || ns.StartsWith(prefix + ".", StringComparison.Ordinal);
    }

    /// <summary>集中保存 0.0.2 相容命名慣例；不將低信心判定偽裝成內建契約。</summary>
    private static bool LegacyMatches(ITypeSymbol type, string role)
    {
        var name = type.Name;
        var arity = (type as INamedTypeSymbol)?.Arity ?? 0;
        return role switch
        {
            "aggregate" => name is "IAggregateRoot" or "AggregateRoot" or "EsAggregateRoot"
                || name.EndsWith("Aggregate", StringComparison.Ordinal) || name.EndsWith("AggregateRoot", StringComparison.Ordinal),
            "entity" => name is "IDomainEntity" or "DomainEntity" || name.EndsWith("Entity", StringComparison.Ordinal),
            "aggregate-repository" => name == "IAggregateRepository" && arity == 2,
            "compatibility-repository" => name == "IDomainRepository" && arity == 2,
            "query-repository" => name == "IQueryRepository" || name.EndsWith("QueryRepository", StringComparison.Ordinal),
            "generic-repository" => arity > 0 && name is "IRepository" or "IGenericRepository" or "IWritableRepository" or "ICrudRepository",
            "repository" => name.EndsWith("Repository", StringComparison.Ordinal),
            "use-case" => name.EndsWith("UseCase", StringComparison.Ordinal),
            "handler" => name.EndsWith("Handler", StringComparison.Ordinal),
            "dispatch-handler" => name.EndsWith("CommandHandler", StringComparison.Ordinal) || name.EndsWith("QueryHandler", StringComparison.Ordinal),
            "controller" => name.EndsWith("Controller", StringComparison.Ordinal),
            "mapper" => name.EndsWith("Mapper", StringComparison.Ordinal),
            "projection" => name == "IProjection",
            "event-sourced-aggregate" => name == "EsAggregateRoot",
            "command" => name == "ICommand",
            "query" => name == "IQuery",
            "domain-service" => name.EndsWith("DomainService", StringComparison.Ordinal),
            "bus" => name is "IMessageBus" or "IMediator" or "IDispatcher" || name.EndsWith("Dispatcher", StringComparison.Ordinal),
            "transport" => InNamespace(type, "Wolverine") || InNamespace(type, "MediatR") || InNamespace(type, "Microsoft.AspNetCore"),
            "db-context" => name.EndsWith("DbContext", StringComparison.Ordinal),
            "db-set" => name == "DbSet",
            _ => false
        };
    }

    /// <summary>判斷 Use Case 相依；合法領域 port 只消除自身的 Dispatcher 名稱誤報。</summary>
    internal bool IsForbiddenUseCaseDependency(ITypeSymbol type)
    {
        if (Is(type, "use-case") || Is(type, "transport")) return true;
        var local = Match(type, "domain-event-dispatcher");
        if (!local.Matched) return Is(type, "bus");
        // Check each other contract separately: a valid local port cannot launder a bus mix-in.
        foreach (var candidate in TypeClosure(type))
        {
            if (Match(candidate, "bus") is { Matched: true, Evidence: "configured-contract" }) return true;
            if (Is(candidate, "transport") || Is(candidate, "use-case")) return true;
            if (!Is(candidate, "domain-event-dispatcher") && Is(candidate, "bus")) return true;
        }
        return false;
    }
}

/// <summary>記錄角色判定來源、信心與契約證據。</summary>
internal sealed class RoleMatch
{
    internal static readonly RoleMatch None = new("", "none", "none", "");
    internal string Role { get; }
    internal string Evidence { get; }
    internal string Confidence { get; }
    internal string Contract { get; }
    internal bool Matched => Evidence != "none";
    /// <summary>建立不可變的角色證據。</summary>
    internal RoleMatch(string role, string evidence, string confidence, string contract)
    { Role = role; Evidence = evidence; Confidence = confidence; Contract = contract; }
}
