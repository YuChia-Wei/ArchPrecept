using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchPrecept.Analyzers.Core;

/// <summary>
/// 驗證 API Controller 的屬性、相依性、持久化存取與直接建立應用程式型別的邊界。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ControllerComplianceAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableHashSet<string> PersistenceMethods =
        ImmutableHashSet.Create(StringComparer.Ordinal, "SaveChanges", "SaveChangesAsync");

    /// <summary>
    /// 取得此分析器支援的 Diagnostic 描述項目。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            RuleDescriptors.ControllerApiAttribute,
            RuleDescriptors.ControllerPersistenceAccess,
            RuleDescriptors.ControllerDirectConstruction,
            RuleDescriptors.ControllerForbiddenDependency);

    /// <summary>
    /// 初始化分析器並註冊 Controller 類別宣告的語法節點分析。
    /// </summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeController, SyntaxKind.ClassDeclaration, SyntaxKind.RecordDeclaration);
    }

    /// <summary>
    /// 針對具 Controller 命名尾碼的具體類別執行所有 Controller 邊界檢查。
    /// </summary>
    private static void AnalyzeController(SyntaxNodeAnalysisContext context)
    {
        var declaration = (TypeDeclarationSyntax)context.Node;
        var controllerSymbol = context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken);
        if (controllerSymbol is null || controllerSymbol.IsAbstract || !RoleClassifier.For(context.Compilation, context.Options).Is(controllerSymbol, "controller"))
        {
            return;
        }

        AnalyzeApiControllerAttribute(context, declaration, controllerSymbol);
        AnalyzePersistenceAccess(context, declaration);
        AnalyzeDirectConstruction(context, declaration);
        AnalyzeInjectedDependencies(context, controllerSymbol);
    }

    /// <summary>
    /// 檢查 Controller 建構子的注入相依性，並回報不允許的型別。
    /// </summary>
    private static void AnalyzeInjectedDependencies(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol controllerSymbol)
    {
        foreach (var constructor in controllerSymbol.InstanceConstructors)
        {
            foreach (var parameter in constructor.Parameters)
            {
                if (!IsForbiddenControllerDependency(parameter.Type, RoleClassifier.For(context.Compilation, context.Options)))
                {
                    continue;
                }

                var location = parameter.Locations.FirstOrDefault() ?? controllerSymbol.Locations[0];
                DiagnosticReporting.Report(context,
                    RuleDescriptors.ControllerForbiddenDependency,
                    location,
                    controllerSymbol.Name,
                    parameter.Type.Name);
            }
        }
    }

    /// <summary>
    /// 判斷注入型別是否違反 Controller 應依賴 Use Case 邊界的規則。
    /// </summary>
    private static bool IsForbiddenControllerDependency(ITypeSymbol type, RoleClassifier classifier)
    {
        return classifier.Is(type, "handler") || classifier.Is(type, "bus")
            || classifier.Is(type, "domain-event-dispatcher") || classifier.Is(type, "domain-service")
            || (classifier.Is(type, "repository") && !classifier.Is(type, "query-repository"));
    }

    /// <summary>
    /// 確認具體 Controller 是否宣告 <c>ApiControllerAttribute</c>。
    /// </summary>
    private static void AnalyzeApiControllerAttribute(
        SyntaxNodeAnalysisContext context,
        TypeDeclarationSyntax declaration,
        INamedTypeSymbol controllerSymbol)
    {
        var hasApiControllerAttribute = controllerSymbol
            .GetAttributes()
            .Any(attribute =>
                attribute.AttributeClass?.Name is "ApiControllerAttribute" or "ApiController");

        if (hasApiControllerAttribute)
        {
            return;
        }

        DiagnosticReporting.Report(context,
            RuleDescriptors.ControllerApiAttribute,
            declaration.Identifier.GetLocation(),
            controllerSymbol.Name);
    }

    /// <summary>
    /// 偵測 Controller 對 <c>DbContext</c> 或持久化寫入方法的直接使用。
    /// </summary>
    private static void AnalyzePersistenceAccess(
        SyntaxNodeAnalysisContext context,
        TypeDeclarationSyntax declaration)
    {
        foreach (var typeSyntax in declaration.DescendantNodes().OfType<TypeSyntax>())
        {
            var type = context.SemanticModel.GetTypeInfo(typeSyntax, context.CancellationToken).Type;
            if (!RoleClassifier.For(context.Compilation, context.Options).Is(type, "db-context"))
            {
                continue;
            }

            DiagnosticReporting.Report(context,
                RuleDescriptors.ControllerPersistenceAccess,
                typeSyntax.GetLocation(),
                declaration.Identifier.ValueText,
                type?.Name ?? typeSyntax.ToString());
        }

        foreach (var invocation in declaration.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            var method = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;
            var methodName = method?.Name ?? GetInvokedMethodName(invocation);
            if (methodName is null || !PersistenceMethods.Contains(methodName))
            {
                continue;
            }

            DiagnosticReporting.Report(context,
                RuleDescriptors.ControllerPersistenceAccess,
                invocation.GetLocation(),
                declaration.Identifier.ValueText,
                methodName);
        }
    }

    /// <summary>
    /// 偵測 Controller 是否直接建立 Handler 或 Use Case 型別。
    /// </summary>
    private static void AnalyzeDirectConstruction(
        SyntaxNodeAnalysisContext context,
        TypeDeclarationSyntax declaration)
    {
        foreach (var creation in declaration.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
        {
            var type = context.SemanticModel.GetTypeInfo(creation, context.CancellationToken).Type;
            var typeName = type?.Name ?? creation.Type.ToString();
            if (!RoleClassifier.For(context.Compilation, context.Options).Is(type, "handler")
                && !RoleClassifier.For(context.Compilation, context.Options).Is(type, "use-case"))
            {
                continue;
            }

            DiagnosticReporting.Report(context,
                RuleDescriptors.ControllerDirectConstruction,
                creation.Type.GetLocation(),
                declaration.Identifier.ValueText,
                typeName);
        }
    }

    /// <summary>
    /// 從呼叫語法取得可辨識的方法名稱。
    /// </summary>
    private static string? GetInvokedMethodName(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            _ => null
        };
    }
}
