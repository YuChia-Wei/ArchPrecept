using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchPrecept.Analyzers.Core;

/// <summary>驗證 Aggregate identity-load/save 與 query port 的有效繼承契約；adapter 依其實作的窄 port 分別評估。</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RepositoryQueryMethodAnalyzer : DiagnosticAnalyzer
{
    /// <summary>建立分析器實例；規則與角色設定由每次編譯提供，不保存 consumer 執行期狀態。</summary>
    public RepositoryQueryMethodAnalyzer() { }

    /// <summary>取得此分析器支援的儲存庫契約 Diagnostic。</summary>
    /// <value>此分析器可回報的描述項；是否啟用及有效嚴重性由本次設定決定。</value>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleDescriptors.RepositoryQueryMethod);

    /// <summary>註冊型別符號分析；略過產生的程式碼。</summary>
    /// <param name="context">Roslyn 提供的註冊環境；由編譯器管理分析與取消生命週期。</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    /// <summary>依角色與每個有效 port 檢查來源型別，將 adapter 的實作方法對回介面能力。</summary>
    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind is not (TypeKind.Interface or TypeKind.Class or TypeKind.Struct)) return;
        var classifier = RoleClassifier.For(context.Compilation, context.Options);
        if (classifier.Configuration.Errors.Count != 0) return;
        if (type.TypeKind == TypeKind.Interface && classifier.IsDirectContract(type, "generic-repository"))
        {
            Report(context, type, type, $"public generic writable repository '{type.Name}' is prohibited");
            return;
        }
        var contracts = RepositoryPortView.AggregateContracts(type, classifier);
        var query = classifier.Is(type, "query-repository");
        if (contracts.IsEmpty && !query) return;
        if (classifier.IsDirectContract(type, "compatibility-repository")
            && !type.AllInterfaces.Any(candidate => classifier.IsDirectContract(candidate, "aggregate-repository")))
            Report(context, type, type, "compatibility repository contract must inherit an aggregate repository contract");
        foreach (var aggregate in contracts.Select(contract => contract.TypeArguments[0]).Distinct<ITypeSymbol>(SymbolEqualityComparer.Default))
            if (!classifier.Is(aggregate, "aggregate"))
                Report(context, type, type, $"aggregate repository type '{aggregate.ToDisplayString()}' is not an Aggregate Root");
        foreach (var contract in contracts)
            if (!RepositoryPortView.HasCompatibleIdentity(contract.TypeArguments[0], contract.TypeArguments[1], classifier))
                Report(context, type, type, $"repository ID '{contract.TypeArguments[1].ToDisplayString()}' does not match its Aggregate identity contract");

        if (type.TypeKind == TypeKind.Interface)
        {
            foreach (var method in RepositoryPortView.GetEffectiveMethods(type))
            {
                if (!contracts.IsEmpty) CheckAggregate(context, type, method, contracts);
                if (query) CheckQuery(context, type, method, classifier);
            }
            return;
        }

        // Each interface view owns its restrictions. A shared adapter may implement separate write/read ports.
        var mapped = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        var checkedAggregate = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        var checkedQuery = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach (var port in type.AllInterfaces)
        {
            var portContracts = RepositoryPortView.AggregateContracts(port, classifier);
            var queryPort = classifier.Is(port, "query-repository");
            if (portContracts.IsEmpty && !queryPort) continue;
            foreach (var method in RepositoryPortView.GetEffectiveMethods(port))
            {
                var implementation = type.FindImplementationForInterfaceMember(method);
                if (implementation is null) continue; // Compiler owns an unimplemented interface member.
                mapped.Add(implementation);
                // The port itself is analyzed when source-owned; external contracts need evidence at this adapter.
                if (!method.Locations.Any(location => location.IsInSource))
                {
                    if (!portContracts.IsEmpty && checkedAggregate.Add(method)) CheckAggregate(context, type, method, portContracts);
                    if (queryPort && checkedQuery.Add(method)) CheckQuery(context, type, method, classifier);
                }
            }
        }
        foreach (var method in RepositoryPortView.GetEffectiveMethods(type))
        {
            if (mapped.Contains(method)) continue;
            if (!contracts.IsEmpty) CheckAggregate(context, type, method, contracts);
            else if (query) CheckQuery(context, type, method, classifier);
        }
    }

    /// <summary>以方法所屬 port 的 Aggregate／ID 替換驗證 identity-load/save 最小簽章。</summary>
    private static void CheckAggregate(SymbolAnalysisContext context, INamedTypeSymbol type, IMethodSymbol method,
        ImmutableArray<INamedTypeSymbol> contracts)
    {
        // A member inherited from an Aggregate port is bound to that port, including its type substitution.
        var ownerContracts = RepositoryPortView.AggregateContracts(method.ContainingType, RoleClassifier.For(context.Compilation, context.Options));
        var candidates = ownerContracts.IsEmpty ? contracts : ownerContracts;
        if (candidates.Any(contract => RepositoryPortView.IsAggregateMethod(method, contract.TypeArguments[0], contract.TypeArguments[1], context.Compilation))) return;
        var reason = method.Name is "FindByIdAsync" or "SaveAsync"
            ? "does not match the approved asynchronous signature"
            : "is outside the FindByIdAsync/SaveAsync contract";
        Report(context, type, method, $"aggregate repository member '{method.Name}' {reason}");
    }

    /// <summary>檢查查詢方法是否宣告寫入動詞或回傳可變領域物件。</summary>
    private static void CheckQuery(SymbolAnalysisContext context, INamedTypeSymbol type, IMethodSymbol method, RoleClassifier classifier)
    {
        if (RepositoryPortView.IsWriteMethod(method.Name))
            Report(context, type, method, $"query repository member '{method.Name}' exposes a persistence write");
        else if (RepositoryPortView.ContainsMutableDomainType(method.ReturnType, classifier))
            Report(context, type, method, $"query repository member '{method.Name}' returns an Aggregate Root or Domain Entity");
    }

    /// <summary>將直接成員違規定位到成員，繼承或來源外違規定位到實際暴露能力的來源型別。</summary>
    private static void Report(SymbolAnalysisContext context, INamedTypeSymbol analyzedType, ISymbol member, string reason)
    {
        var location = SymbolEqualityComparer.Default.Equals(member.ContainingType, analyzedType)
            ? member.Locations.FirstOrDefault(candidate => candidate.IsInSource) : null;
        location ??= analyzedType.Locations.FirstOrDefault(candidate => candidate.IsInSource);
        if (location is not null) DiagnosticReporting.Report(context, RuleDescriptors.RepositoryQueryMethod, location, reason);
    }
}
