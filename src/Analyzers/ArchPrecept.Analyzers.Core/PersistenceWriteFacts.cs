using System.Linq;
using Microsoft.CodeAnalysis;

namespace ArchPrecept.Analyzers.Core;

/// <summary>辨識所選持久化契約實際宣告的寫入 API，不把衍生型別的新同名方法當成該 API。</summary>
internal static class PersistenceWriteFacts
{
    /// <summary>檢查方法、覆寫鏈與顯式／隱式介面實作所對應的寫入契約。</summary>
    internal static bool IsWrite(IMethodSymbol method, RoleClassifier classifier)
    {
        for (var current = method; current is not null; current = current.OverriddenMethod)
        {
            if (IsDeclaredWrite(current, classifier)) return true;
            foreach (var contract in current.ContainingType.AllInterfaces)
            foreach (var member in contract.GetMembers().OfType<IMethodSymbol>())
            {
                if (!IsDeclaredWrite(member, classifier)) continue;
                var implementation = current.ContainingType.FindImplementationForInterfaceMember(member);
                if (SymbolEqualityComparer.Default.Equals(implementation?.OriginalDefinition, current.OriginalDefinition)) return true;
            }
        }
        return false;
    }

    /// <summary>名稱集合只套用於真正契約根；明選命名慣例仍保留其低信心語意。</summary>
    private static bool IsDeclaredWrite(IMethodSymbol method, RoleClassifier classifier)
    {
        if (method.IsStatic) return false;
        var name = method.Name;
        if (name is "SaveChanges" or "SaveChangesAsync")
            return IsDirectRole(method.ContainingType, "db-context", classifier);
        return name is "Add" or "AddAsync" or "Update" or "Remove"
            && (IsDirectRole(method.ContainingType, "db-context", classifier)
                || IsDirectRole(method.ContainingType, "db-set", classifier));
    }

    /// <summary>繼承角色不授予任意新增方法相同能力；已選 convention 依原有名稱契約處理。</summary>
    private static bool IsDirectRole(INamedTypeSymbol type, string role, RoleClassifier classifier) =>
        classifier.IsDirectContract(type, role)
        || classifier.Match(type, role) is { Matched: true, Evidence: "naming-convention" };
}
