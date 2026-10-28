using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchPrecept.Analyzers.Core;

/// <summary>針對精確辨識的 ValueObject 檢查有界 instance 狀態與 Entity 參考。</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ValueObjectAnalyzer : DiagnosticAnalyzer
{
    /// <summary>建立分析器實例；規則與角色設定由每次編譯提供，不保存 consumer 執行期狀態。</summary>
    public ValueObjectAnalyzer() { }

    /// <summary>取得兩條獨立 VO 診斷與未評估狀態診斷。</summary>
    /// <value>此分析器可回報的描述項；是否啟用及有效嚴重性由本次設定決定。</value>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        RuleDescriptors.ValueObjectMutableState, RuleDescriptors.ValueObjectEntityReference, ConfigurationCoverageAnalyzer.Coverage);

    /// <summary>註冊具名型別分析，略過 generated code。</summary>
    /// <param name="context">Roslyn 提供的註冊環境；由編譯器管理分析與取消生命週期。</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(Analyze, SymbolKind.NamedType);
    }

    /// <summary>分析 VO 的有效 instance 成員，對未知部分回報原因而不宣稱深層不可變。</summary>
    private static void Analyze(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        var classifier = RoleClassifier.For(context.Compilation, context.Options);
        var match = classifier.Match(type, "value-object");
        if (!match.Matched || match.Confidence != "high") return;
        var checkMutable = classifier.Configuration.IsEnabled("DBA1018") && classifier.Configuration.Severity("DBA1018") != "none";
        var checkEntities = classifier.Configuration.IsEnabled("DBA1019") && classifier.Configuration.Severity("DBA1019") != "none";
        if (!checkMutable && !checkEntities) return;
        foreach (var member in StateMembers(type))
        {
            // The base declaration is checked once when it is a source VO in this compilation.
            if (!SymbolEqualityComparer.Default.Equals(member.ContainingType, type)
                && member.ContainingType.Locations.Any(location => location.IsInSource)
                && classifier.Match(member.ContainingType, "value-object").Confidence == "high"
                && !ConfigurationCoverageAnalyzer.IsGenerated(member.ContainingType)) continue;
            var location = member.Locations.FirstOrDefault(candidate => candidate.IsInSource)
                ?? type.Locations.FirstOrDefault(candidate => candidate.IsInSource) ?? Location.None;
            var memberType = MemberType(member);
            var path = member.ContainingType.Name + "." + member.Name;
            if (checkMutable)
            {
                var result = IsWritable(member) ? Inspection.Failed(path, "ordinary setter or non-readonly field")
                    : Inspect(memberType, path, classifier, false, new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default), 0);
                Report(context, type, location, match, "DBA1018", result);
            }
            if (checkEntities)
            {
                var result = Inspect(memberType, path, classifier, true, new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default), 0);
                Report(context, type, location, match, "DBA1019", result);
            }
        }
    }

    /// <summary>依檢查結果回報違規，或回報尚未評估的宣告型別路徑。</summary>
    private static void Report(SymbolAnalysisContext context, INamedTypeSymbol owner, Location location, RoleMatch match, string id, Inspection result)
    {
        if (result.Kind == "failed")
        {
            if (id == "DBA1018")
                DiagnosticReporting.Report(context, RuleDescriptors.ValueObjectMutableState, location, owner.Name, result.Path, result.Reason);
            else
                DiagnosticReporting.Report(context, RuleDescriptors.ValueObjectEntityReference, location, owner.Name, result.Path);
        }
        else if (result.Kind == "not-evaluated")
            context.ReportDiagnostic(ConfigurationCoverageAnalyzer.Status(location, owner.ToDisplayString(), "value-object", id,
                "not-evaluated", result.Path + ": " + result.Reason, match));
    }

    /// <summary>分析實際宣告的狀態、已知容器與限制；以 visited set 及深度上限避免循環。</summary>
    private static Inspection Inspect(ITypeSymbol type, string path, RoleClassifier classifier, bool entityCheck,
        HashSet<ITypeSymbol> visited, int depth)
    {
        if (entityCheck && classifier.Match(type, "entity") is { Matched: true, Confidence: "high" })
            return Inspection.Failed(path + " -> " + type.ToDisplayString(), "Entity reference");
        if (IsScalar(type)) return Inspection.Passed;
        if (depth >= 16 || !visited.Add(type)) return Inspection.Unknown(path, "cyclic type or depth limit 16");
        try
        {
            if (type is IArrayTypeSymbol array)
                return entityCheck ? Inspect(array.ElementType, path + "[]", classifier, true, visited, depth + 1)
                    : Inspection.Failed(path, "array exposes mutable elements");
            if (type is ITypeParameterSymbol) return Inspection.Unknown(path, "generic constraint does not prove complete state");
            if (type is not INamedTypeSymbol named || type.SpecialType == SpecialType.System_Object || type.TypeKind == TypeKind.Dynamic)
                return Inspection.Unknown(path, "object, dynamic or unresolved type");
            if (named.IsTupleType)
                return InspectChildren(named.TupleElements.Select(field => (field.Type, path + "." + field.Name)), classifier, entityCheck, visited, depth);
            if (named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                return Inspect(named.TypeArguments[0], path + ".Value", classifier, entityCheck, visited, depth + 1);

            var container = IsKnownContainer(named) ? named
                : RoleClassifier.TypeClosure(named).OfType<INamedTypeSymbol>().Where(IsKnownContainer)
                    .OrderByDescending(IsMutableContainer).FirstOrDefault();
            if (container is not null)
            {
                if (!entityCheck && IsMutableContainer(container)) return Inspection.Failed(path, "known mutable collection " + container.Name);
                if (!entityCheck && !RoleClassifier.InNamespace(container, "System.Collections.Immutable"))
                    return Inspection.Unknown(path, "read-only collection interface does not prove backing-state immutability");
                return InspectChildren(container.TypeArguments.Select((argument, index) =>
                    (argument, path + "<" + index + ":" + argument.ToDisplayString() + ">")), classifier, entityCheck, visited, depth);
            }

            if (named.DeclaringSyntaxReferences.Length == 0 || ConfigurationCoverageAnalyzer.IsGenerated(named))
                return Inspection.Unknown(path, "unknown metadata/generated type or wrapper; generic arguments do not prove stored state");
            var unknown = Inspection.Passed;
            foreach (var member in StateMembers(named))
            {
                var childPath = path + "." + member.Name;
                if (!entityCheck && IsWritable(member)) return Inspection.Failed(childPath, "ordinary setter or non-readonly field");
                var result = Inspect(MemberType(member), childPath, classifier, entityCheck, visited, depth + 1);
                if (result.Kind == "failed") return result;
                if (result.Kind == "not-evaluated") unknown = result;
            }
            return unknown;
        }
        finally { visited.Remove(type); }
    }

    /// <summary>掃描容器的實際元素型別，已知違規優先於未評估狀態。</summary>
    private static Inspection InspectChildren(IEnumerable<(ITypeSymbol Type, string Path)> children, RoleClassifier classifier,
        bool entityCheck, HashSet<ITypeSymbol> visited, int depth)
    {
        var unknown = Inspection.Passed;
        foreach (var child in children)
        {
            var result = Inspect(child.Type, child.Path, classifier, entityCheck, visited, depth + 1);
            if (result.Kind == "failed") return result;
            if (result.Kind == "not-evaluated") unknown = result;
        }
        return unknown;
    }

    /// <summary>列出自身與基底的 instance 屬性／欄位，排除 backing field 與編譯器合成狀態。</summary>
    private static IEnumerable<ISymbol> StateMembers(INamedTypeSymbol type)
    {
        for (var current = type; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
        {
            foreach (var member in current.GetMembers())
            {
                if (member.IsStatic || member.IsImplicitlyDeclared) continue;
                if (member is IPropertySymbol { IsIndexer: false } || member is IFieldSymbol { IsConst: false, AssociatedSymbol: null })
                    yield return member;
            }
        }
    }

    /// <summary>取得屬性或欄位的宣告型別。</summary>
    private static ITypeSymbol MemberType(ISymbol member) => member is IPropertySymbol property ? property.Type : ((IFieldSymbol)member).Type;

    /// <summary>所有一般 setter 都可變；init 及 readonly 純量本身不構成違規。</summary>
    private static bool IsWritable(ISymbol member) => member is IPropertySymbol { SetMethod: { IsInitOnly: false } }
        || member is IFieldSymbol { IsReadOnly: false };

    /// <summary>辨識內建純量、enum 與已知標準不可變值型別。</summary>
    private static bool IsScalar(ITypeSymbol type) => type.TypeKind == TypeKind.Enum
        || type.SpecialType is >= SpecialType.System_Boolean and <= SpecialType.System_Decimal
        || type.SpecialType == SpecialType.System_String
        || (type.ContainingNamespace?.ToDisplayString() == "System"
            && type.ContainingAssembly?.Identity.Name is "System.Runtime" or "System.Private.CoreLib" or "mscorlib"
            && type.Name is "Guid" or "DateTime" or "DateTimeOffset" or "TimeSpan" or "DateOnly" or "TimeOnly");

    /// <summary>只將已知 BCL collection 的型別引數解讀成元素，不猜測任意 generic wrapper。</summary>
    private static bool IsKnownContainer(INamedTypeSymbol type)
    {
        if (type.TypeArguments.Length == 0) return false;
        var assembly = type.ContainingAssembly.Identity.Name;
        if (assembly is not ("System.Runtime" or "System.Collections" or "System.Private.CoreLib" or "mscorlib" or "System.Collections.Immutable" or "System.ObjectModel")) return false;
        var ns = type.ContainingNamespace.ToDisplayString();
        return ns == "System.Collections.Generic" && type.Name is
            "List" or "Dictionary" or "HashSet" or "SortedSet" or "SortedDictionary" or "SortedList" or "Queue" or "Stack" or "LinkedList"
            or "IEnumerable" or "ICollection" or "IList" or "IDictionary" or "ISet" or "IReadOnlyCollection" or "IReadOnlyList" or "IReadOnlyDictionary" or "IReadOnlySet"
            || ns == "System.Collections.Immutable" && type.Name is "ImmutableArray" or "ImmutableList" or "ImmutableHashSet" or "ImmutableDictionary" or "ImmutableSortedDictionary" or "ImmutableSortedSet" or "ImmutableQueue" or "ImmutableStack" or "IImmutableList" or "IImmutableSet" or "IImmutableDictionary" or "IImmutableQueue" or "IImmutableStack"
            || ns == "System.Collections.ObjectModel" && type.Name is "Collection" or "ObservableCollection" or "ReadOnlyCollection" or "ReadOnlyDictionary";
    }

    /// <summary>辨識允許寫入操作的集合契約與實作。</summary>
    private static bool IsMutableContainer(INamedTypeSymbol type) => type.Name is
        "List" or "Dictionary" or "HashSet" or "SortedSet" or "SortedDictionary" or "SortedList" or "Queue" or "Stack" or "LinkedList"
        or "ICollection" or "IList" or "IDictionary" or "ISet" or "Collection" or "ObservableCollection";

    /// <summary>保存有界型別檢查的結果與可讀路徑。</summary>
    private sealed class Inspection
    {
        internal static readonly Inspection Passed = new("evaluated", "", "");
        internal string Kind { get; }
        internal string Path { get; }
        internal string Reason { get; }
        /// <summary>建立型別檢查結果。</summary>
        private Inspection(string kind, string path, string reason) { Kind = kind; Path = path; Reason = reason; }
        /// <summary>建立已確認的違規結果。</summary>
        internal static Inspection Failed(string path, string reason) => new("failed", path, reason);
        /// <summary>建立未能完成的評估結果。</summary>
        internal static Inspection Unknown(string path, string reason) => new("not-evaluated", path, reason);
    }
}
