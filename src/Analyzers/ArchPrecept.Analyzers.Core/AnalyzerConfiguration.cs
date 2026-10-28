using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchPrecept.Analyzers.Core;

/// <summary>保存依固定優先序合併的 Analyzer 設定；錯誤不會當作正常零診斷。</summary>
internal sealed class AnalyzerConfiguration
{
    internal static readonly ImmutableHashSet<string> Roles = ImmutableHashSet.Create(StringComparer.Ordinal,
        "aggregate", "entity", "value-object", "aggregate-repository", "query-repository", "repository",
        "compatibility-repository", "generic-repository", "use-case", "handler", "dispatch-handler", "controller",
        "mapper", "projection", "event-sourced-aggregate", "command", "query", "domain-service",
        "domain-event-dispatcher", "integration-event-publisher", "outbox-port", "query-service", "service-locator",
        "bus", "transport", "db-context", "db-set");
    internal static readonly ImmutableHashSet<string> ProfileNames = ImmutableHashSet.Create(StringComparer.Ordinal,
        "legacy", "core", "web-api", "messaging", "event-sourcing", "framework", "ddd");

    internal ImmutableHashSet<string> Profiles { get; private set; } = ImmutableHashSet.Create("legacy");
    internal bool NamingConventions { get; private set; } = true;
    internal Dictionary<string, ContractMapping> Mappings { get; } = new(StringComparer.Ordinal);
    internal Dictionary<string, NamingConvention> Conventions { get; } = new(StringComparer.Ordinal);
    internal Dictionary<string, string> Expected { get; } = new(StringComparer.Ordinal);
    internal Dictionary<string, RuleSetting> Rules { get; } = new(StringComparer.Ordinal);
    internal Dictionary<string, PureQuerySelection> PureQueries { get; } = new(StringComparer.Ordinal);
    internal List<string> Errors { get; } = new();

    /// <summary>按 repository、project 順序讀取明確加入 AdditionalFiles 的兩層設定。</summary>
    internal static AnalyzerConfiguration Load(AnalyzerOptions options)
    {
        var result = new AnalyzerConfiguration();
        foreach (var name in new[] { "archprecept.repository.xml", "archprecept.project.xml" })
        {
            var files = options.AdditionalFiles.Where(file =>
                string.Equals(Path.GetFileName(file.Path), name, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (files.Length > 1)
            {
                result.Errors.Add($"{name}: expected one AdditionalFile, found {files.Length}.");
                continue;
            }
            if (files.Length == 1)
            {
                var content = files[0].GetText();
                if (content is null)
                    result.Errors.Add($"{name}: cannot read AdditionalFile.");
                else
                    result.Merge(content.ToString(), name);
            }
        }
        return result;
    }

    /// <summary>驗證 Schema 後合併單層設定；非法設定不會部分套用。</summary>
    private void Merge(string text, string source)
    {
        try
        {
            using var reader = XmlReader.Create(new StringReader(text), new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = 262144
            });
            var document = XDocument.Load(reader);
            var schema = new XmlSchemaSet { XmlResolver = null };
            using var stream = typeof(AnalyzerConfiguration).Assembly.GetManifestResourceStream("ArchPrecept.configuration.xsd");
            using var schemaReader = XmlReader.Create(stream!);
            schema.Add(null, schemaReader);
            var errors = new List<string>();
            document.Validate(schema, (_, args) => errors.Add(args.Message));
            if (errors.Count != 0)
            {
                Errors.AddRange(errors.Select(error => $"{source}: {error}"));
                return;
            }
            var root = document.Root!;
            var profiles = root.Attribute("profiles")?.Value;
            var selected = profiles?.Split(',').Select(value => value.Trim()).ToImmutableHashSet(StringComparer.Ordinal);
            if (selected is not null && selected.Any(profile => !ProfileNames.Contains(profile)))
                errors.Add("profiles: use legacy, core, web-api, messaging, event-sourcing, framework or ddd.");
            foreach (var group in root.Elements().GroupBy(element => element.Name.LocalName + ":" +
                         (string?)(element.Attribute("name") ?? element.Attribute("type") ?? element.Attribute("role") ?? element.Attribute("id"))
                         + (element.Name == "PureQuery" ? "," + (string?)element.Attribute("assembly") : "")))
                if (group.Count() > 1) errors.Add($"Duplicate entry {group.Key}.");
            foreach (var element in root.Elements())
            {
                var role = (string?)(element.Attribute("name") ?? element.Attribute("role"));
                if (role is not null && !Roles.Contains(role)) errors.Add($"Unknown role '{role}'.");
                if (element.Name == "Rule" && !RuleCatalog.Contains((string)element.Attribute("id")!))
                    errors.Add($"Unknown rule '{element.Attribute("id")!.Value}'.");
                if (element.Name == "Convention" && element.Attribute("suffix") is null && element.Attribute("namespace") is null)
                    errors.Add("Convention requires suffix or namespace.");
            }
            if (errors.Count != 0)
            {
                Errors.AddRange(errors.Select(error => $"{source}: {error}"));
                return;
            }
            if (selected is not null) Profiles = selected;
            if (root.Attribute("namingConventions") is { } naming) NamingConventions = (bool)naming;
            foreach (var element in root.Elements())
            {
                switch (element.Name.LocalName)
                {
                    case "Role":
                        Mappings[(string)element.Attribute("name")!] = new ContractMapping(
                            (string)element.Attribute("metadataName")!, (string)element.Attribute("assembly")!);
                        break;
                    case "Convention":
                        Conventions[(string)element.Attribute("role")!] = new NamingConvention(
                            (string?)element.Attribute("suffix"), (string?)element.Attribute("namespace"));
                        break;
                    case "Expected":
                        Expected[(string)element.Attribute("type")!] = (string)element.Attribute("role")!;
                        break;
                    case "PureQuery":
                        var selection = new PureQuerySelection((string)element.Attribute("type")!,
                            (string)element.Attribute("assembly")!, (bool)element.Attribute("enabled")!);
                        PureQueries[selection.MetadataName + "," + selection.AssemblyName] = selection;
                        break;
                    case "Rule":
                        var id = (string)element.Attribute("id")!;
                        Rules.TryGetValue(id, out var existing);
                        Rules[id] = new RuleSetting(
                            element.Attribute("enabled") is { } enabled ? (bool?)enabled : existing?.Enabled,
                            (string?)element.Attribute("severity") ?? existing?.Severity);
                        break;
                }
            }
        }
        catch (Exception error) when (error is XmlException or XmlSchemaException or InvalidOperationException)
        {
            Errors.Add($"{source}: {error.Message}");
        }
    }

    /// <summary>取得 Profile 與個別規則 override 合併後的啟用狀態。</summary>
    internal bool IsEnabled(string id)
    {
        if (Errors.Count != 0) return false;
        if (Rules.TryGetValue(id, out var setting) && setting.Enabled.HasValue) return setting.Enabled.Value;
        return RuleCatalog.GetProfiles(id).Any(Profiles.Contains);
    }

    /// <summary>取得個別規則 severity；標準 Roslyn severity 仍由編譯器最後處理。</summary>
    internal string? Severity(string id) => Rules.TryGetValue(id, out var setting) ? setting.Severity : null;
}

/// <summary>明選整個 Controller 的 pure-query 例外；不傳遞至衍生型別或任意 action。</summary>
internal sealed class PureQuerySelection
{
    internal string MetadataName { get; }
    internal string AssemblyName { get; }
    internal bool Enabled { get; }
    /// <summary>保存明確型別、組件與兩層設定覆寫值。</summary>
    internal PureQuerySelection(string metadataName, string assemblyName, bool enabled)
    { MetadataName = metadataName; AssemblyName = assemblyName; Enabled = enabled; }
}

/// <summary>保存明確契約的 metadata 名稱及組件名稱。</summary>
internal sealed class ContractMapping
{
    internal string MetadataName { get; }
    internal string AssemblyName { get; }
    /// <summary>建立明確契約對照。</summary>
    internal ContractMapping(string metadataName, string assemblyName)
    { MetadataName = metadataName; AssemblyName = assemblyName; }
}

/// <summary>保存可選的名稱尾碼與 namespace 區段慣例。</summary>
internal sealed class NamingConvention
{
    internal string? Suffix { get; }
    internal string? Namespace { get; }
    /// <summary>建立名稱慣例。</summary>
    internal NamingConvention(string? suffix, string? @namespace) { Suffix = suffix; Namespace = @namespace; }
}

/// <summary>保存個別規則的明確覆寫。</summary>
internal sealed class RuleSetting
{
    internal bool? Enabled { get; }
    internal string? Severity { get; }
    /// <summary>建立啟用與嚴重性覆寫。</summary>
    internal RuleSetting(bool? enabled, string? severity) { Enabled = enabled; Severity = severity; }
}
