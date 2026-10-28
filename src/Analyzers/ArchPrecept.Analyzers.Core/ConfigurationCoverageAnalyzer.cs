using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchPrecept.Analyzers.Core;

/// <summary>回報無效設定及採用者明確宣告型別的有界評估狀態。</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConfigurationCoverageAnalyzer : DiagnosticAnalyzer
{
    /// <summary>建立分析器實例；規則與角色設定由每次編譯提供，不保存 consumer 執行期狀態。</summary>
    public ConfigurationCoverageAnalyzer() { }

    internal static readonly DiagnosticDescriptor InvalidConfiguration = new("DBA0001", "Invalid ArchPrecept configuration",
        "ArchPrecept configuration: {0}", "Configuration", DiagnosticSeverity.Error, true,
        helpLinkUri: DocumentationLinks.For("docs/diagnostics/DBA0001.md"),
        customTags: new[] { WellKnownDiagnosticTags.NotConfigurable, WellKnownDiagnosticTags.CompilationEnd });
    internal static readonly DiagnosticDescriptor Coverage = new("DBA0002", "ArchPrecept evaluation scope",
        "{0}: {1} ({2})", "Coverage", DiagnosticSeverity.Info, true,
        helpLinkUri: DocumentationLinks.For("docs/diagnostics/DBA0002.md"));

    /// <summary>取得設定與覆蓋狀態診斷。</summary>
    /// <value>此分析器可回報的描述項；是否啟用及有效嚴重性由本次設定決定。</value>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(InvalidConfiguration, Coverage);

    /// <summary>註冊編譯結束回報，不因沒有來源符號而遺漏無效設定。</summary>
    /// <param name="context">Roslyn 提供的註冊環境；由編譯器管理分析與取消生命週期。</param>
    public override void Initialize(AnalysisContext context)
    {
        // Report exclusions at generated declarations; this analyzer does not run product rules.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(Analyze);
    }

    /// <summary>分別回報設定錯誤與已宣告的各規則適用狀態。</summary>
    private static void Analyze(CompilationAnalysisContext context)
    {
        var classifier = RoleClassifier.For(context.Compilation, context.Options);
        var config = classifier.Configuration;
        foreach (var error in config.Errors)
            context.ReportDiagnostic(Diagnostic.Create(InvalidConfiguration, Location.None, error));
        foreach (var expected in config.Expected)
        {
            var type = context.Compilation.GetTypeByMetadataName(expected.Key);
            var location = type?.Locations.FirstOrDefault(candidate => candidate.IsInSource) ?? Location.None;
            var match = classifier.Match(type, expected.Value);
            var rules = RuleCatalog.Ids.Where(id => RuleCatalog.GetRoles(id).Contains(expected.Value)).OrderBy(id => id).ToArray();
            if (config.Errors.Count != 0 || type is null || !match.Matched)
            {
                context.ReportDiagnostic(Status(location, expected.Key, expected.Value, "", "not-evaluated",
                    config.Errors.Count != 0 ? "invalid configuration" : type is null ? "type is missing or ambiguous" : "role is not recognized", match));
                continue;
            }
            if (rules.Length == 0)
                context.ReportDiagnostic(Status(location, expected.Key, expected.Value, "", "not-applicable", "no rule targets this role", match));
            foreach (var id in rules)
            {
                var state = "applicable";
                var reason = "role and declaration are eligible; execution and conformance are not attested by this scope report";
                if (!config.IsEnabled(id) || config.Severity(id) == "none" || IsSuppressed(context, type, id))
                { state = "disabled"; reason = "profile, rule override or compiler severity disables this rule"; }
                else if (!type.Locations.Any(candidate => candidate.IsInSource) || IsGenerated(type))
                { state = "not-evaluated"; reason = "metadata-only or generated declaration is excluded"; }
                else if (id is "DBA1018" or "DBA1019" && match.Confidence != "high")
                { state = "not-evaluated"; reason = "ValueObject rules require an explicit or built-in contract, not a naming convention"; }
                else if (!Supports(type, id))
                { state = "not-applicable"; reason = "this rule does not analyze this declaration kind or abstract shape"; }
                context.ReportDiagnostic(Status(location, expected.Key, expected.Value, id, state, reason, match));
            }
        }
    }

    /// <summary>檢查標準 Roslyn 對檔案或 compilation 的停用設定。</summary>
    private static bool IsSuppressed(CompilationAnalysisContext context, INamedTypeSymbol type, string id)
    {
        if (context.Compilation.Options.SpecificDiagnosticOptions.TryGetValue(id, out var setting) && setting == ReportDiagnostic.Suppress)
            return true;
        return type.DeclaringSyntaxReferences.Any(reference =>
            (context.Compilation.Options.SyntaxTreeOptionsProvider?.TryGetDiagnosticValue(
                reference.SyntaxTree, id, context.CancellationToken, out var effective) == true && effective == ReportDiagnostic.Suppress)
            || (context.Options.AnalyzerConfigOptionsProvider.GetOptions(reference.SyntaxTree)
                .TryGetValue("dotnet_diagnostic." + id + ".severity", out var value) && value == "none"));
    }

    /// <summary>明確列出各檢查支援的宣告形狀，避免 interface 或 abstract 被當成完整執行。</summary>
    private static bool Supports(INamedTypeSymbol type, string id)
    {
        if (id == "DBA1015") return type.TypeKind == TypeKind.Interface || !type.IsAbstract;
        if (id == "DBA1016") return type.TypeKind is TypeKind.Class or TypeKind.Struct;
        if (id is "DBA1001" or "DBA1018" or "DBA1019") return true;
        if (id is "DBA1004" or "DBA1005" or "DBA1006" or "DBA1014")
            return type.TypeKind == TypeKind.Class && !type.IsAbstract;
        return type.TypeKind is TypeKind.Class or TypeKind.Struct;
    }

    /// <summary>辨識來源產生器與工具產生的型別，維持規則略過 generated code 的契約。</summary>
    internal static bool IsGenerated(INamedTypeSymbol type) =>
        type.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() is
            "System.CodeDom.Compiler.GeneratedCodeAttribute" or "System.Runtime.CompilerServices.CompilerGeneratedAttribute")
        || (type.DeclaringSyntaxReferences.Length > 0 && type.DeclaringSyntaxReferences.All(reference =>
        {
            var path = reference.SyntaxTree.FilePath;
            if (new[] { ".g.cs", ".g.i.cs", ".generated.cs", ".designer.cs" }.Any(suffix => path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))) return true;
            var text = reference.SyntaxTree.GetText();
            return text.ToString(new Microsoft.CodeAnalysis.Text.TextSpan(0, Math.Min(text.Length, 2048)))
                .IndexOf("<auto-generated", StringComparison.OrdinalIgnoreCase) >= 0;
        }));

    /// <summary>建立含機器可讀狀態與角色證據的覆蓋診斷。</summary>
    internal static Diagnostic Status(Location location, string type, string role, string id, string status, string reason, RoleMatch match)
    {
        var properties = ImmutableDictionary<string, string?>.Empty.Add("type", type).Add("role", role)
            .Add("diagnosticId", id).Add("ruleId", RuleCatalog.Contains(id) ? RuleCatalog.Value(id, "id") : "")
            .Add("status", status).Add("reason", reason).Add("roleEvidence", match.Evidence).Add("confidence", match.Confidence);
        return Diagnostic.Create(Coverage, location, properties, type + (id.Length == 0 ? "" : " / " + id), status, reason);
    }
}
