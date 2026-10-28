using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ArchPrecept.Analyzers.Core;

/// <summary>以依賴型別實際暴露的成員建立 repository 視圖；不推論方法本體是否有副作用。</summary>
internal static class RepositoryPortView
{
    private static readonly string[] WritePrefixes =
        { "Save", "Add", "Create", "Insert", "Update", "Delete", "Remove", "Upsert", "Persist", "Purge" };

    /// <summary>取得介面的有效繼承方法或 concrete 型別的公開方法，保留泛型替換。</summary>
    internal static ImmutableArray<IMethodSymbol> GetEffectiveMethods(INamedTypeSymbol type)
    {
        var result = ImmutableArray.CreateBuilder<IMethodSymbol>();
        var seen = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        var owners = type.TypeKind == TypeKind.Interface
            ? new[] { type }.Concat(type.AllInterfaces)
            : BaseTypes(type);
        foreach (var owner in owners)
        foreach (var method in owner.GetMembers().OfType<IMethodSymbol>())
        {
            if (method.MethodKind != MethodKind.Ordinary || method.IsImplicitlyDeclared
                || method.DeclaredAccessibility != Accessibility.Public) continue;
            if (seen.Add(method)) result.Add(method);
        }
        return result.ToImmutable();
    }

    /// <summary>列舉應用程式基底型別，不納入 System.Object 的一般物件能力。</summary>
    private static IEnumerable<INamedTypeSymbol> BaseTypes(INamedTypeSymbol type)
    {
        for (var current = type; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
            yield return current;
    }

    /// <summary>取得每個 Aggregate／ID 配對；不將第一個 port 的型別套用到其他 port。</summary>
    internal static ImmutableArray<INamedTypeSymbol> AggregateContracts(INamedTypeSymbol type, RoleClassifier classifier) =>
        RoleClassifier.TypeClosure(type).OfType<INamedTypeSymbol>()
            .Where(candidate => candidate.TypeArguments.Length == 2
                && (classifier.IsDirectContract(candidate, "aggregate-repository")
                    || classifier.IsDirectContract(candidate, "compatibility-repository"))).ToImmutableArray();

    /// <summary>確認 query-only 依賴視圖；明選入口例外仍由呼叫端另外驗證。</summary>
    internal static bool IsQueryOnly(ITypeSymbol type, RoleClassifier classifier)
    {
        if (type is not INamedTypeSymbol named || type.TypeKind == TypeKind.Error
            || classifier.Configuration.Errors.Count != 0 || !(classifier.Is(type, "query-repository") || classifier.Is(type, "query-service"))
            || classifier.Is(type, "aggregate-repository") || classifier.Is(type, "compatibility-repository")
            || classifier.Is(type, "generic-repository") || classifier.Is(type, "integration-event-publisher")
            || classifier.Is(type, "outbox-port")) return false;
        if (RoleClassifier.TypeClosure(type).Any(candidate => candidate.TypeKind == TypeKind.Error)) return false;
        // A writable property/event also exposes a mutation even without a write-named method.
        var owners = named.TypeKind == TypeKind.Interface ? new[] { named }.Concat(named.AllInterfaces) : BaseTypes(named);
        if (owners.SelectMany(owner => owner.GetMembers()).Any(member =>
            member is IPropertySymbol property && property.DeclaredAccessibility == Accessibility.Public
                && (property.SetMethod is not null && property.SetMethod.DeclaredAccessibility == Accessibility.Public
                    || property.IsStatic || property.ReturnsByRef || property.ReturnsByRefReadonly
                    || HasUnresolvedType(property.Type) || ContainsMutableDomainType(property.Type, classifier)
                    || ReturnsCapability(property.Type, classifier)
                    || property.Parameters.Any(parameter => HasUnresolvedType(parameter.Type)))
            || member is IFieldSymbol field && field.DeclaredAccessibility == Accessibility.Public
                && (!field.IsReadOnly && !field.IsConst || HasUnresolvedType(field.Type) || ContainsMutableDomainType(field.Type, classifier)
                    || ReturnsCapability(field.Type, classifier))
            || member is IEventSymbol && member.DeclaredAccessibility == Accessibility.Public)) return false;
        return GetEffectiveMethods(named).All(method => !method.IsStatic && !method.ReturnsByRef && !method.ReturnsByRefReadonly
            && !HasUnresolvedType(method.ReturnType) && method.Parameters.All(parameter => parameter.RefKind == RefKind.None && !HasUnresolvedType(parameter.Type))
            && !IsWriteMethod(method.Name) && !ContainsMutableDomainType(method.ReturnType, classifier)
            && !ReturnsCapability(method.ReturnType, classifier));
    }

    /// <summary>查詢不能透過回傳型別或泛型 wrapper 轉交寫入、編排、派送或傳輸能力。</summary>
    private static bool ReturnsCapability(ITypeSymbol type, RoleClassifier classifier) =>
        classifier.Is(type, "repository") || classifier.Is(type, "domain-service")
        || classifier.Is(type, "bus") || classifier.Is(type, "transport") || classifier.Is(type, "use-case")
        || classifier.Is(type, "handler") || classifier.Is(type, "domain-event-dispatcher")
        || classifier.Is(type, "integration-event-publisher") || classifier.Is(type, "outbox-port")
        || type is IArrayTypeSymbol array && ReturnsCapability(array.ElementType, classifier)
        || type is INamedTypeSymbol named && named.TypeArguments.Any(argument => ReturnsCapability(argument, classifier));

    /// <summary>當 Aggregate identity 契約可辨識時，確認 port 的 ID 引數與該契約一致。</summary>
    internal static bool HasCompatibleIdentity(ITypeSymbol aggregate, ITypeSymbol id, RoleClassifier classifier)
    {
        var identities = RoleClassifier.TypeClosure(aggregate).OfType<INamedTypeSymbol>()
            .Where(candidate => candidate.TypeArguments.Length == 1 && classifier.IsDirectContract(candidate, "aggregate"))
            .Select(candidate => candidate.TypeArguments[0]).ToArray();
        return identities.Length == 0 || identities.All(identity => SymbolEqualityComparer.Default.Equals(identity, id));
    }

    /// <summary>以文件列明的動詞集合辨識宣告寫入能力，不檢查方法本體。</summary>
    internal static bool IsWriteMethod(string name) => WritePrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal));

    /// <summary>檢查回傳型別及其陣列／泛型引數中的 Entity／Aggregate。</summary>
    internal static bool ContainsMutableDomainType(ITypeSymbol type, RoleClassifier classifier) =>
        ContainsMutableDomainType(type, classifier, new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default));

    /// <summary>沿宣告型別的陣列與泛型引數查找可變領域角色，避免循環重複走訪。</summary>
    private static bool ContainsMutableDomainType(ITypeSymbol type, RoleClassifier classifier, HashSet<ITypeSymbol> visited)
    {
        if (!visited.Add(type)) return false;
        if (classifier.Is(type, "aggregate") || classifier.Is(type, "entity")) return true;
        if (type is IArrayTypeSymbol array) return ContainsMutableDomainType(array.ElementType, classifier, visited);
        return type is INamedTypeSymbol named && named.TypeArguments.Any(argument => ContainsMutableDomainType(argument, classifier, visited));
    }

    /// <summary>錯誤型別不構成 query-only 證據。</summary>
    private static bool HasUnresolvedType(ITypeSymbol type) => type.TypeKind == TypeKind.Error
        || type is IArrayTypeSymbol array && HasUnresolvedType(array.ElementType)
        || type is INamedTypeSymbol named && named.TypeArguments.Any(HasUnresolvedType);

    /// <summary>確認最小 identity-load/save 形狀，Task 與 CancellationToken 必須是 BCL 符號。</summary>
    internal static bool IsAggregateMethod(IMethodSymbol method, ITypeSymbol aggregate, ITypeSymbol id, Compilation compilation)
    {
        if (method.IsStatic || method.Arity != 0 || method.ReturnsByRef || method.ReturnsByRefReadonly
            || method.Parameters.Length < 1 || method.Parameters.Length > 2
            || method.Parameters.Any(parameter => parameter.RefKind != RefKind.None || parameter.IsParams)) return false;
        if (method.Parameters.Length == 2 && !IsBcl(method.Parameters[1].Type, "System.Threading.CancellationToken", compilation)) return false;
        if (method.Name == "FindByIdAsync")
            return SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, id)
                && method.ReturnType is INamedTypeSymbol task && IsBcl(task.OriginalDefinition, "System.Threading.Tasks.Task`1", compilation)
                && SymbolEqualityComparer.Default.Equals(task.TypeArguments[0], aggregate);
        return method.Name == "SaveAsync" && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, aggregate)
            && IsBcl(method.ReturnType, "System.Threading.Tasks.Task", compilation);
    }

    /// <summary>從編譯器核心程式庫取得符號，避免 consumer 的 System 同名型別取代 BCL。</summary>
    private static bool IsBcl(ITypeSymbol type, string metadataName, Compilation compilation)
    {
        var core = compilation.GetSpecialType(SpecialType.System_Object).ContainingAssembly;
        var expected = core.GetTypeByMetadataName(metadataName);
        if (expected is null)
            expected = compilation.GetTypeByMetadataName(metadataName);
        return expected is not null && expected.ContainingAssembly.Identity.PublicKeyToken.Length != 0
            && SymbolEqualityComparer.Default.Equals(type, expected);
    }
}
