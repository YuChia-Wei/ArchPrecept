using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ArchPrecept.Analyzers.Core;

/// <summary>
/// 驗證 Use Case 與派送 Handler 的相依性、契約形狀及傳輸邊界。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseCaseServiceProviderInjectionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>建立分析器實例；規則與角色設定由每次編譯提供，不保存 consumer 執行期狀態。</summary>
    public UseCaseServiceProviderInjectionAnalyzer() { }

    /// <summary>
    /// 取得此分析器支援的 Diagnostic 描述項目。
    /// </summary>
    /// <value>此分析器可回報的描述項；是否啟用及有效嚴重性由本次設定決定。</value>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            RuleDescriptors.UseCaseServiceProviderInjection,
            RuleDescriptors.UseCaseDependencyResolution,
            RuleDescriptors.MixedCommandQueryHandler,
            RuleDescriptors.UseCaseDirectRepositoryConstruction,
            RuleDescriptors.UseCaseContractShape,
            RuleDescriptors.UseCaseForbiddenDependency,
            RuleDescriptors.DispatchHandlerBoundary);

    /// <summary>
    /// 初始化分析器並註冊具名型別及物件建立運算式的分析。
    /// </summary>
    /// <param name="context">Roslyn 提供的註冊環境；由編譯器管理分析與取消生命週期。</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression, SyntaxKind.ImplicitObjectCreationExpression);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    /// <summary>
    /// 分析 Use Case 契約、Use Case 實作與派送 Handler 的形狀及相依性。
    /// </summary>
    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind == TypeKind.Interface && RoleClassifier.For(context.Compilation, context.Options).Is(type, "use-case"))
        {
            AnalyzeUseCaseContract(context, type);
            return;
        }

        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct) || !(RoleClassifier.For(context.Compilation, context.Options).Is(type, "use-case") || RoleClassifier.For(context.Compilation, context.Options).Is(type, "handler")))
        {
            return;
        }

        if (RoleClassifier.For(context.Compilation, context.Options).Is(type, "use-case"))
        {
            AnalyzeUseCaseImplementation(context, type);
        }

        if (RoleClassifier.For(context.Compilation, context.Options).Is(type, "dispatch-handler"))
        {
            AnalyzeDispatchHandler(context, type);
        }

        var handlesCommand = false;
        var handlesQuery = false;

        foreach (var member in type.GetMembers())
        {
            if (member.IsImplicitlyDeclared) continue;
            if (member is IFieldSymbol field)
            {
                ReportForbiddenMemberType(context, type, field.Type, field.Locations, field.Name);
                continue;
            }

            if (member is IPropertySymbol property)
            {
                ReportForbiddenMemberType(context, type, property.Type, property.Locations, property.Name);
                ReportInjectionAttributes(context, type, property);
                continue;
            }

            if (member is not IMethodSymbol method)
            {
                continue;
            }
            if (method.MethodKind is not (MethodKind.Ordinary or MethodKind.Constructor or MethodKind.ExplicitInterfaceImplementation)) continue;

            foreach (var parameter in method.Parameters)
            {
                ReportForbiddenMemberType(context, type, parameter.Type, parameter.Locations, parameter.Name);
            }

            if (method.Name is not "Handle" and not "HandleAsync" || method.Parameters.Length == 0)
            {
                continue;
            }

            handlesCommand |= RoleClassifier.For(context.Compilation, context.Options).Is(method.Parameters[0].Type, "command");
            handlesQuery |= RoleClassifier.For(context.Compilation, context.Options).Is(method.Parameters[0].Type, "query");
        }

        if (handlesCommand && handlesQuery)
        {
            DiagnosticReporting.Report(context,
                RuleDescriptors.MixedCommandQueryHandler,
                type.Locations[0],
                type.Name);
        }
    }

    /// <summary>
    /// 確認 Use Case 介面只宣告一個符合規範的 <c>ExecuteAsync</c> 操作。
    /// </summary>
    private static void AnalyzeUseCaseContract(SymbolAnalysisContext context, INamedTypeSymbol type)
    {
        var methods = RepositoryPortView.GetEffectiveMethods(type).ToArray();
        var executeMethods = methods.Where(method => method.Name == "ExecuteAsync").ToArray();

        if (executeMethods.Length != 1)
        {
            ReportUseCaseShape(context, type, "declare exactly one ExecuteAsync operation");
            return;
        }

        if (methods.Any(method => method.Name != "ExecuteAsync"))
        {
            ReportUseCaseShape(context, type, "must expose only its canonical ExecuteAsync operation");
        }

        AnalyzeExecuteAsyncShape(context, type, executeMethods[0]);
    }

    /// <summary>
    /// 確認具體 Use Case 實作的操作形狀及建構子相依性符合規範。
    /// </summary>
    private static void AnalyzeUseCaseImplementation(SymbolAnalysisContext context, INamedTypeSymbol type)
    {
        if (type.IsAbstract)
        {
            return;
        }

        var executeMethods = EffectiveImplementationMethods(type)
            .Where(method => method.Name == "ExecuteAsync" || method.ExplicitInterfaceImplementations.Any(contract => contract.Name == "ExecuteAsync"))
            .ToArray();
        if (executeMethods.Length != 1)
        {
            ReportUseCaseShape(context, type, "implement exactly one ExecuteAsync operation");
        }
        else
        {
            AnalyzeExecuteAsyncShape(context, type, executeMethods[0]);
        }

        if (EffectiveImplementationMethods(type)
            .Any(method => method.Name is "Handle" or "HandleAsync"
                || method.ExplicitInterfaceImplementations.Any(contract => contract.Name is "Handle" or "HandleAsync")))
        {
            ReportUseCaseShape(context, type, "must not also expose a Handler entry point");
        }

    }

    /// <summary>
    /// 驗證 <c>ExecuteAsync</c> 的回傳型別、取消權杖與輸入參數形狀。
    /// </summary>
    private static void AnalyzeExecuteAsyncShape(
        SymbolAnalysisContext context,
        INamedTypeSymbol type,
        IMethodSymbol method)
    {
        if (method.IsStatic || method.IsGenericMethod || method.Parameters.Any(parameter => parameter.RefKind != RefKind.None))
            ReportUseCaseShape(context, type, "ExecuteAsync must be an instance, non-generic operation with value parameters");
        if (!IsTask(method.ReturnType, context.Compilation))
        {
            ReportUseCaseShape(context, type, "ExecuteAsync must return Task or Task<T>");
        }
        else if (method.ReturnType is INamedTypeSymbol { IsGenericType: true } taskType
            && taskType.TypeArguments.Length == 1
            && ContainsTransport(taskType.TypeArguments[0], RoleClassifier.For(context.Compilation, context.Options), true))
        {
            ReportUseCaseShape(context, type, "HTTP, broker acknowledgement, or framework output is forbidden");
        }

        if (method.Parameters.Length == 0
            || !KnownSymbols.IsBcl(method.Parameters[method.Parameters.Length - 1].Type, context.Compilation, "System.Threading.CancellationToken")
            || method.Parameters[method.Parameters.Length - 1].IsOptional)
        {
            ReportUseCaseShape(context, type, "ExecuteAsync must end with a non-optional CancellationToken");
            return;
        }

        var inputParameters = method.Parameters.Take(method.Parameters.Length - 1).ToArray();
        if (inputParameters.Length > 1)
        {
            ReportUseCaseShape(context, type, "multiple input values require one dedicated Input object");
            return;
        }

        if (inputParameters.Any(parameter => ContainsTransport(parameter.Type, RoleClassifier.For(context.Compilation, context.Options), true)))
        {
            ReportUseCaseShape(context, type, "transport input or a directly exposed transport member is forbidden");
            return;
        }

        if (inputParameters.Length == 1
            && !IsStandardScalar(inputParameters[0].Type, context.Compilation)
            && !inputParameters[0].Type.Name.EndsWith("Input", System.StringComparison.Ordinal))
        {
            ReportUseCaseShape(context, type, "a non-scalar input type must use the Input suffix");
        }
    }

    /// <summary>
    /// 驗證 Command 或 Query Handler 僅調適一個 Use Case，且不依賴儲存庫或領域服務。
    /// </summary>
    private static void AnalyzeDispatchHandler(SymbolAnalysisContext context, INamedTypeSymbol type)
    {
        var dependencies = DeclaredDependencies(type).ToArray();
        var useCaseDependencies = dependencies
            .Where(dependency => RoleClassifier.For(context.Compilation, context.Options).Is(dependency, "use-case"))
            .Distinct(SymbolEqualityComparer.Default)
            .Count();

        if (useCaseDependencies != 1)
        {
            DiagnosticReporting.Report(context,
                RuleDescriptors.DispatchHandlerBoundary,
                type.Locations[0],
                type.Name,
                "depend on exactly one Use Case");
        }

        if (dependencies.Any(dependency => RoleClassifier.For(context.Compilation, context.Options).Is(dependency, "repository") || RoleClassifier.For(context.Compilation, context.Options).Is(dependency, "domain-service")))
        {
            DiagnosticReporting.Report(context,
                RuleDescriptors.DispatchHandlerBoundary,
                type.Locations[0],
                type.Name,
                "must not depend on repositories or Domain services");
        }
    }

    /// <summary>
    /// 以指定細節回報 Use Case 契約形狀違規。
    /// </summary>
    private static void ReportUseCaseShape(
        SymbolAnalysisContext context,
        INamedTypeSymbol type,
        string detail)
    {
        DiagnosticReporting.Report(context,
            RuleDescriptors.UseCaseContractShape,
            type.Locations[0],
            type.Name,
            detail);
    }

    /// <summary>
    /// 偵測 Use Case 或 Handler 是否直接建立 Repository 型別。
    /// </summary>
    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        var creation = (BaseObjectCreationExpressionSyntax)context.Node;
        var containingType = context.ContainingSymbol?.ContainingType;
        if (containingType is null || !(RoleClassifier.For(context.Compilation, context.Options).Is(containingType, "use-case") || RoleClassifier.For(context.Compilation, context.Options).Is(containingType, "handler")))
        {
            return;
        }

        var createdType = context.SemanticModel.GetTypeInfo(creation, context.CancellationToken).Type;
        if (createdType is null || !RoleClassifier.For(context.Compilation, context.Options).Is(createdType, "repository"))
        {
            return;
        }

        DiagnosticReporting.Report(context,
            RuleDescriptors.UseCaseDirectRepositoryConstruction,
            creation is ObjectCreationExpressionSyntax explicitCreation ? explicitCreation.Type.GetLocation() : creation.GetLocation(),
            containingType.Name,
            createdType.Name);
    }

    /// <summary>
    /// 檢查欄位、屬性或參數型別，並回報服務提供者或服務定位器的使用。
    /// </summary>
    private static void ReportForbiddenMemberType(
        SymbolAnalysisContext context,
        INamedTypeSymbol containingType,
        ITypeSymbol memberType,
        ImmutableArray<Location> locations,
        string memberName)
    {
        var location = locations.Length > 0 ? locations[0] : containingType.Locations[0];
        var classifier = RoleClassifier.For(context.Compilation, context.Options);
        if (classifier.Is(containingType, "use-case") && classifier.IsForbiddenUseCaseDependency(memberType))
            DiagnosticReporting.Report(context, RuleDescriptors.UseCaseForbiddenDependency, location, containingType.Name, memberType.Name);
        if (KnownSymbols.IsBcl(memberType, context.Compilation, "System.IServiceProvider"))
        {
            DiagnosticReporting.Report(context,
                RuleDescriptors.UseCaseServiceProviderInjection,
                location,
                containingType.Name);
        }
        else if (classifier.Is(memberType, "service-locator"))
        {
            DiagnosticReporting.Report(context,
                RuleDescriptors.UseCaseDependencyResolution,
                location,
                containingType.Name,
                memberName);
        }
    }

    /// <summary>
    /// 偵測屬性上的服務注入屬性並回報相依性解析違規。
    /// </summary>
    private static void ReportInjectionAttributes(
        SymbolAnalysisContext context,
        INamedTypeSymbol containingType,
        IPropertySymbol property)
    {
        foreach (var attribute in property.GetAttributes())
        {
            var attributeType = attribute.AttributeClass;
            if (!KnownSymbols.IsPlatform(attributeType, context.Compilation, "Microsoft.AspNetCore.Components.InjectAttribute", "Microsoft.AspNetCore.Components")
                && !KnownSymbols.IsPlatform(attributeType, context.Compilation, "Microsoft.AspNetCore.Mvc.FromServicesAttribute", "Microsoft.AspNetCore.Mvc.Core"))
            {
                continue;
            }

            DiagnosticReporting.Report(context,
                RuleDescriptors.UseCaseDependencyResolution,
                property.Locations[0],
                containingType.Name,
                attributeType!.Name);
        }
    }

    /// <summary>
    /// 判斷型別是否為 <c>System.Threading.Tasks.Task</c> 或其泛型形式。
    /// </summary>
    private static bool IsTask(ITypeSymbol type, Compilation compilation) =>
        KnownSymbols.IsBcl(type, compilation, "System.Threading.Tasks.Task")
        || KnownSymbols.IsBcl(type, compilation, "System.Threading.Tasks.Task`1");

    /// <summary>
    /// 判斷型別是否為可直接作為 Use Case 輸入的標準純量型別。
    /// </summary>
    private static bool IsStandardScalar(ITypeSymbol type, Compilation compilation)
    {
        return type.SpecialType is SpecialType.System_Boolean
            or SpecialType.System_Byte
            or SpecialType.System_SByte
            or SpecialType.System_Int16
            or SpecialType.System_UInt16
            or SpecialType.System_Int32
            or SpecialType.System_UInt32
            or SpecialType.System_Int64
            or SpecialType.System_UInt64
            or SpecialType.System_Single
            or SpecialType.System_Double
            or SpecialType.System_Decimal
            or SpecialType.System_Char
            or SpecialType.System_String
            || new[] { "System.Guid", "System.DateTime", "System.DateTimeOffset", "System.DateOnly", "System.TimeOnly" }
                .Any(metadataName => KnownSymbols.IsBcl(type, compilation, metadataName));
    }

    /// <summary>
    /// 檢查型別／陣列／泛型引數，以及一層公開 instance field/property 的 transport 角色。
    /// </summary>
    private static bool ContainsTransport(ITypeSymbol type, RoleClassifier classifier, bool inspectMembers,
        HashSet<ITypeSymbol>? visited = null, int depth = 0)
    {
        visited ??= new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        if (depth > 8 || !visited.Add(type)) return false;
        if (classifier.Is(type, "transport")) return true;
        if (type is IArrayTypeSymbol array) return ContainsTransport(array.ElementType, classifier, inspectMembers, visited, depth + 1);
        if (type is not INamedTypeSymbol named) return false;
        if (named.TypeArguments.Any(argument => ContainsTransport(argument, classifier, inspectMembers, visited, depth + 1))) return true;
        return inspectMembers && RoleClassifier.TypeClosure(named).OfType<INamedTypeSymbol>()
            .SelectMany(candidate => candidate.GetMembers())
            .Where(member => !member.IsStatic && member.DeclaredAccessibility == Accessibility.Public)
            .Any(member => member switch
            {
                IPropertySymbol property => ContainsTransport(property.Type, classifier, false, visited, depth + 1),
                IFieldSymbol field => ContainsTransport(field.Type, classifier, false, visited, depth + 1),
                _ => false
            });
    }

    /// <summary>包含繼承後可見的方法與顯式實作，依 override 或同簽章 hiding 保留最衍生公開操作。</summary>
    private static IEnumerable<IMethodSymbol> EffectiveImplementationMethods(INamedTypeSymbol type)
    {
        var contracts = type.AllInterfaces.SelectMany(contract => contract.GetMembers().OfType<IMethodSymbol>())
            .Select(type.FindImplementationForInterfaceMember).OfType<IMethodSymbol>();
        var methods = RepositoryPortView.GetEffectiveMethods(type).Concat(contracts)
            .Distinct<IMethodSymbol>(SymbolEqualityComparer.Default).ToArray();
        return methods.Where(method => !methods.Any(other => Overrides(other, method) || HidesSameSignature(other, method)));
    }

    /// <summary>公開的同簽章方法隱藏基底操作；不同 overload 與顯式介面操作仍分開計算。</summary>
    private static bool HidesSameSignature(IMethodSymbol candidate, IMethodSymbol baseMethod)
    {
        if (candidate.DeclaredAccessibility != Accessibility.Public || baseMethod.DeclaredAccessibility != Accessibility.Public
            || candidate.Name != baseMethod.Name || candidate.Arity != baseMethod.Arity
            || candidate.Parameters.Length != baseMethod.Parameters.Length) return false;
        for (var index = 0; index < candidate.Parameters.Length; index++)
            if (candidate.Parameters[index].RefKind != baseMethod.Parameters[index].RefKind
                || !SymbolEqualityComparer.Default.Equals(candidate.Parameters[index].Type, baseMethod.Parameters[index].Type)) return false;
        for (var parent = candidate.ContainingType.BaseType; parent is not null; parent = parent.BaseType)
            if (SymbolEqualityComparer.Default.Equals(parent, baseMethod.ContainingType)) return true;
        return false;
    }

    /// <summary>避免把覆寫與其基底宣告重複計算為兩個操作。</summary>
    private static bool Overrides(IMethodSymbol candidate, IMethodSymbol baseMethod)
    {
        for (var current = candidate.OverriddenMethod; current is not null; current = current.OverriddenMethod)
            if (SymbolEqualityComparer.Default.Equals(current, baseMethod)) return true;
        return false;
    }

    /// <summary>計算宣告依賴型別，不將其數量聲稱為執行時呼叫次數。</summary>
    private static IEnumerable<ITypeSymbol> DeclaredDependencies(INamedTypeSymbol type) =>
        type.GetMembers().Where(member => !member.IsImplicitlyDeclared).SelectMany(member => member switch
        {
            IFieldSymbol field => new[] { field.Type },
            IPropertySymbol property => new[] { property.Type },
            IMethodSymbol method => method.Parameters.Select(parameter => parameter.Type),
            _ => Enumerable.Empty<ITypeSymbol>()
        });

    /// <summary>補捉方法體直接呼叫禁止 port 的位置；不追蹤跨方法／動態／反射資料流。</summary>
    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var owner = context.ContainingSymbol.ContainingType;
        var classifier = RoleClassifier.For(context.Compilation, context.Options);
        if (owner is null || !classifier.Is(owner, "use-case")) return;
        var dependency = invocation.Instance?.Type ?? invocation.TargetMethod.ContainingType;
        if (SymbolEqualityComparer.Default.Equals(dependency, owner) || !classifier.IsForbiddenUseCaseDependency(dependency)) return;
        DiagnosticReporting.Report(context, RuleDescriptors.UseCaseForbiddenDependency, invocation.Syntax.GetLocation(), owner.Name, dependency.Name);
    }
}
