using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EngineeringGuardrails.Analyzers.Core;

/// <summary>
/// 驗證事件溯源聚合根的狀態僅能在 <c>When</c> 事件轉換中變更。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EventSourcedAggregateMutationAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> AssignmentKinds =
        ImmutableArray.Create(
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxKind.AddAssignmentExpression,
            SyntaxKind.SubtractAssignmentExpression,
            SyntaxKind.MultiplyAssignmentExpression,
            SyntaxKind.DivideAssignmentExpression,
            SyntaxKind.ModuloAssignmentExpression,
            SyntaxKind.AndAssignmentExpression,
            SyntaxKind.ExclusiveOrAssignmentExpression,
            SyntaxKind.OrAssignmentExpression,
            SyntaxKind.LeftShiftAssignmentExpression,
            SyntaxKind.RightShiftAssignmentExpression,
            SyntaxKind.CoalesceAssignmentExpression);

    /// <summary>
    /// 取得此分析器支援的 Diagnostic 描述項目。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(RuleDescriptors.EventSourcedAggregateDirectMutation);

    /// <summary>
    /// 初始化分析器並註冊指定指派與遞增遞減語法的分析。
    /// </summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeAssignment, AssignmentKinds);
        context.RegisterSyntaxNodeAction(
            AnalyzeIncrementOrDecrement,
            SyntaxKind.PreIncrementExpression,
            SyntaxKind.PreDecrementExpression,
            SyntaxKind.PostIncrementExpression,
            SyntaxKind.PostDecrementExpression);
    }

    /// <summary>
    /// 將指派運算式的左側目標交由狀態變更檢查處理。
    /// </summary>
    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;
        AnalyzeMutation(context, assignment.Left);
    }

    /// <summary>
    /// 擷取遞增或遞減運算式的運算元並檢查是否變更聚合狀態。
    /// </summary>
    private static void AnalyzeIncrementOrDecrement(SyntaxNodeAnalysisContext context)
    {
        var expression = (ExpressionSyntax)context.Node;
        var operand = expression switch
        {
            PrefixUnaryExpressionSyntax prefix => prefix.Operand,
            PostfixUnaryExpressionSyntax postfix => postfix.Operand,
            _ => null
        };

        if (operand is not null)
        {
            AnalyzeMutation(context, operand);
        }
    }

    /// <summary>
    /// 判斷狀態變更目標是否為事件溯源聚合根且發生在允許的 <c>When</c> 轉換之外。
    /// </summary>
    private static void AnalyzeMutation(SyntaxNodeAnalysisContext context, ExpressionSyntax target)
    {
        var method = target.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method is null || method.Identifier.ValueText == "When")
        {
            return;
        }

        var classDeclaration = target.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        var aggregateSymbol = classDeclaration is null
            ? null
            : context.SemanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);
        if (aggregateSymbol is null || !InheritsFromEsAggregateRoot(aggregateSymbol))
        {
            return;
        }

        var member = context.SemanticModel.GetSymbolInfo(target, context.CancellationToken).Symbol;
        if (member is not IFieldSymbol and not IPropertySymbol
            || member.ContainingType is null
            || !IsAggregateMember(aggregateSymbol, member.ContainingType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            RuleDescriptors.EventSourcedAggregateDirectMutation,
            target.GetLocation(),
            aggregateSymbol.Name,
            member.Name));
    }

    /// <summary>
    /// 判斷型別是否繼承自 <c>EsAggregateRoot</c>。
    /// </summary>
    private static bool InheritsFromEsAggregateRoot(INamedTypeSymbol symbol)
    {
        for (var current = symbol.BaseType; current is not null; current = current.BaseType)
        {
            if (current.Name == "EsAggregateRoot")
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 判斷成員宣告型別是否位於指定聚合根的繼承階層中。
    /// </summary>
    private static bool IsAggregateMember(INamedTypeSymbol aggregate, INamedTypeSymbol declaringType)
    {
        for (var current = aggregate; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, declaringType))
            {
                return true;
            }
        }

        return false;
    }
}
