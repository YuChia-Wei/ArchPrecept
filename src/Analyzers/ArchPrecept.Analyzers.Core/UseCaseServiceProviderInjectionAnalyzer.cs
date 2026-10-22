using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchPrecept.Analyzers.Core;

/// <summary>
/// 驗證 Use Case 與派送 Handler 的相依性、契約形狀及傳輸邊界。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseCaseServiceProviderInjectionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// 取得此分析器支援的 Diagnostic 描述項目。
    /// </summary>
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
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, Microsoft.CodeAnalysis.CSharp.SyntaxKind.ObjectCreationExpression);
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
        var methods = type.GetMembers().OfType<IMethodSymbol>()
            .Where(method => method.MethodKind == MethodKind.Ordinary)
            .ToArray();
        var executeMethods = methods.Where(method => method.Name == "ExecuteAsync").ToArray();

        if (executeMethods.Length != 1)
        {
            ReportUseCaseShape(context, type, "declare exactly one ExecuteAsync operation");
            return;
        }

        if (methods.Any(method => method.Name is "Handle" or "HandleAsync"))
        {
            ReportUseCaseShape(context, type, "must not expose Handle or HandleAsync");
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

        var executeMethods = type.GetMembers("ExecuteAsync").OfType<IMethodSymbol>()
            .Where(method => method.MethodKind == MethodKind.Ordinary)
            .ToArray();
        if (executeMethods.Length != 1)
        {
            ReportUseCaseShape(context, type, "implement exactly one ExecuteAsync operation");
        }
        else
        {
            AnalyzeExecuteAsyncShape(context, type, executeMethods[0]);
        }

        if (type.GetMembers().OfType<IMethodSymbol>()
            .Any(method => method.Name is "Handle" or "HandleAsync"))
        {
            ReportUseCaseShape(context, type, "must not also expose a Handler entry point");
        }

        foreach (var constructor in type.InstanceConstructors)
        {
            foreach (var parameter in constructor.Parameters)
            {
                if (!RoleClassifier.For(context.Compilation, context.Options).IsForbiddenUseCaseDependency(parameter.Type))
                {
                    continue;
                }

                DiagnosticReporting.Report(context,
                    RuleDescriptors.UseCaseForbiddenDependency,
                    parameter.Locations.FirstOrDefault() ?? type.Locations[0],
                    type.Name,
                    parameter.Type.Name);
            }
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
        if (!IsTask(method.ReturnType))
        {
            ReportUseCaseShape(context, type, "ExecuteAsync must return Task or Task<T>");
        }
        else if (method.ReturnType is INamedTypeSymbol { IsGenericType: true } taskType
            && taskType.TypeArguments.Length == 1
            && IsTransportOutput(taskType.TypeArguments[0]))
        {
            ReportUseCaseShape(context, type, "HTTP, broker acknowledgement, or framework output is forbidden");
        }

        if (method.Parameters.Length == 0
            || !IsCancellationToken(method.Parameters[method.Parameters.Length - 1].Type)
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

        if (inputParameters.Any(parameter => IsTransportContract(parameter.Type, RoleClassifier.For(context.Compilation, context.Options))))
        {
            ReportUseCaseShape(context, type, "transport, Command, Query, Request, Message, or Contract input is forbidden");
            return;
        }

        if (inputParameters.Length == 1
            && !IsStandardScalar(inputParameters[0].Type)
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
        var dependencies = type.InstanceConstructors
            .SelectMany(constructor => constructor.Parameters)
            .Concat(type.GetMembers().OfType<IMethodSymbol>().SelectMany(method => method.Parameters))
            .Select(parameter => parameter.Type)
            .ToArray();
        var useCaseDependencies = dependencies
            .Where(dependency => RoleClassifier.For(context.Compilation, context.Options).Is(dependency, "use-case"))
            .Select(dependency => dependency.ToDisplayString())
            .Distinct()
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
        var creation = (Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax)context.Node;
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
            creation.Type.GetLocation(),
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
        if (memberType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.IServiceProvider")
        {
            DiagnosticReporting.Report(context,
                RuleDescriptors.UseCaseServiceProviderInjection,
                location,
                containingType.Name);
        }
        else if (memberType.Name.EndsWith("ServiceLocator", System.StringComparison.Ordinal))
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
            var attributeName = attribute.AttributeClass?.Name;
            if (attributeName is not "Inject" and not "InjectAttribute"
                and not "FromServices" and not "FromServicesAttribute")
            {
                continue;
            }

            DiagnosticReporting.Report(context,
                RuleDescriptors.UseCaseDependencyResolution,
                property.Locations[0],
                containingType.Name,
                attributeName);
        }
    }

    /// <summary>
    /// 判斷型別是否為 <c>System.Threading.Tasks.Task</c> 或其泛型形式。
    /// </summary>
    private static bool IsTask(ITypeSymbol type)
    {
        return type.Name == "Task"
            && type.ContainingNamespace?.ToDisplayString() == "System.Threading.Tasks";
    }

    /// <summary>
    /// 判斷型別是否為 <c>System.Threading.CancellationToken</c>。
    /// </summary>
    private static bool IsCancellationToken(ITypeSymbol type)
    {
        return type.Name == "CancellationToken"
            && type.ContainingNamespace?.ToDisplayString() == "System.Threading";
    }

    /// <summary>
    /// 判斷型別是否為可直接作為 Use Case 輸入的標準純量型別。
    /// </summary>
    private static bool IsStandardScalar(ITypeSymbol type)
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
            || type.Name is "Guid" or "DateTime" or "DateTimeOffset" or "DateOnly" or "TimeOnly";
    }

    /// <summary>
    /// 判斷型別是否為傳輸層的 Request、Command、Query、Message 或 Contract。
    /// </summary>
    private static bool IsTransportContract(ITypeSymbol type, RoleClassifier classifier)
    {
        var name = type.Name;
        return name.EndsWith("Request", System.StringComparison.Ordinal)
            || name.EndsWith("Command", System.StringComparison.Ordinal)
            || name.EndsWith("Query", System.StringComparison.Ordinal)
            || name.EndsWith("Message", System.StringComparison.Ordinal)
            || name.EndsWith("Contract", System.StringComparison.Ordinal)
            || classifier.Is(type, "command") || classifier.Is(type, "query");
    }

    /// <summary>
    /// 判斷輸出型別是否為 HTTP、訊息代理或框架專屬的傳輸結果。
    /// </summary>
    private static bool IsTransportOutput(ITypeSymbol type)
    {
        var name = type.Name;
        var namespaceName = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        return name is "IActionResult" or "IResult"
            || name.EndsWith("Acknowledgement", System.StringComparison.Ordinal)
            || name.EndsWith("Envelope", System.StringComparison.Ordinal)
            || namespaceName.StartsWith("Microsoft.AspNetCore", System.StringComparison.Ordinal)
            || namespaceName.StartsWith("Wolverine", System.StringComparison.Ordinal);
    }

}
