using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchPrecept.Analyzers.Core;

/// <summary>統一套用 Profile、規則覆寫與角色證據，不重複維護各 Analyzer 的設定。</summary>
internal static class DiagnosticReporting
{
    /// <summary>回報符號分析結果，保留共同的規則與角色 metadata。</summary>
    internal static void Report(SymbolAnalysisContext context, DiagnosticDescriptor descriptor, Location location, params object[] arguments)
    {
        var subject = context.Symbol as INamedTypeSymbol ?? context.Symbol.ContainingType;
        var diagnostic = Create(RoleClassifier.For(context.Compilation, context.Options), descriptor, location, subject, arguments);
        if (diagnostic is not null) context.ReportDiagnostic(diagnostic);
    }

    /// <summary>回報語法分析結果，從診斷位置取得實際所屬型別。</summary>
    internal static void Report(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, Location location, params object[] arguments)
    {
        var declaration = context.Node.AncestorsAndSelf().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault();
        var subject = declaration is null ? context.ContainingSymbol?.ContainingType
            : context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) as INamedTypeSymbol;
        var diagnostic = Create(RoleClassifier.For(context.Compilation, context.Options), descriptor, location, subject, arguments);
        if (diagnostic is not null) context.ReportDiagnostic(diagnostic);
    }

    /// <summary>建立已套用啟用與自訂 severity 的診斷；編譯器會再套用標準 severity 設定。</summary>
    private static Diagnostic? Create(RoleClassifier classifier, DiagnosticDescriptor descriptor, Location location,
        INamedTypeSymbol? subject, object[] arguments)
    {
        var config = classifier.Configuration;
        if (!config.IsEnabled(descriptor.Id) || config.Severity(descriptor.Id) == "none") return null;
        var evidence = RuleCatalog.GetRoles(descriptor.Id).Select(role => classifier.Match(subject, role))
            .FirstOrDefault(match => match.Matched) ?? RoleMatch.None;
        var properties = ImmutableDictionary<string, string?>.Empty
            .Add("ruleId", RuleCatalog.Value(descriptor.Id, "id"))
            .Add("diagnosticId", descriptor.Id).Add("status", "failed")
            .Add("role", evidence.Role).Add("roleEvidence", evidence.Evidence).Add("confidence", evidence.Confidence)
            .Add("contract", evidence.Contract).Add("type", subject?.ToDisplayString());
        var severity = config.Severity(descriptor.Id);
        if (severity is not null)
        {
            var selected = severity == "error" ? DiagnosticSeverity.Error
                : severity == "warning" ? DiagnosticSeverity.Warning
                : severity == "suggestion" ? DiagnosticSeverity.Info : DiagnosticSeverity.Hidden;
            descriptor = new DiagnosticDescriptor(descriptor.Id, descriptor.Title, descriptor.MessageFormat,
                descriptor.Category, selected, true, descriptor.Description, descriptor.HelpLinkUri, descriptor.CustomTags.ToArray());
        }
        return Diagnostic.Create(descriptor, location, properties, arguments);
    }
}
