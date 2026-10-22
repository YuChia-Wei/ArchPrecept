using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace ArchPrecept.Analyzers.Core;

/// <summary>從唯一的機器可讀 Catalog 建立 Descriptor 與追溯資料。</summary>
internal static class RuleCatalog
{
    private static readonly ImmutableDictionary<string, XElement> Rules = Load();

    /// <summary>載入內嵌 Catalog，部署時不需要其他產品或設定服務。</summary>
    private static ImmutableDictionary<string, XElement> Load()
    {
        using var stream = typeof(RuleCatalog).Assembly.GetManifestResourceStream("ArchPrecept.catalog.xml");
        return XDocument.Load(stream!).Root!.Elements("Rule")
            .ToImmutableDictionary(rule => (string)rule.Attribute("diagnostic")!, StringComparer.Ordinal);
    }

    /// <summary>確認公開 Diagnostic ID 已登錄。</summary>
    internal static bool Contains(string id) => Rules.ContainsKey(id);
    /// <summary>列出全部公開 Diagnostic ID。</summary>
    internal static IEnumerable<string> Ids => Rules.Keys;
    /// <summary>取得指定規則的單一文字欄位。</summary>
    internal static string Value(string id, string key) => (string)Rules[id].Attribute(key)!;
    /// <summary>取得適用 Profile。</summary>
    internal static string[] GetProfiles(string id) => Value(id, "profiles").Split(',');
    /// <summary>取得適用角色。</summary>
    internal static string[] GetRoles(string id) => Value(id, "roles").Split(',');
    /// <summary>由 Catalog 建立診斷描述項與對應的說明連結。</summary>
    internal static DiagnosticDescriptor Descriptor(string id) => new(
        id, Value(id, "title"), Value(id, "message"), DiagnosticCategories.Architecture,
        (DiagnosticSeverity)Enum.Parse(typeof(DiagnosticSeverity), Value(id, "severity")), true,
        Value(id, "description"), "https://github.com/YuChia-Wei/ArchPrecept/blob/main/" + Value(id, "document"));
}
