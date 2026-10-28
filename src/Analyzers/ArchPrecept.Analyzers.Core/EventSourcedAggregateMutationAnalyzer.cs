using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ArchPrecept.Analyzers.Core;

/// <summary>
/// 驗證事件溯源聚合根的狀態僅能在 <c>When</c> 事件轉換中變更。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EventSourcedAggregateMutationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>建立分析器實例；規則與角色設定由每次編譯提供，不保存 consumer 執行期狀態。</summary>
    public EventSourcedAggregateMutationAnalyzer() { }

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
            SyntaxKind.UnsignedRightShiftAssignmentExpression,
            SyntaxKind.CoalesceAssignmentExpression);

    /// <summary>
    /// 取得此分析器支援的 Diagnostic 描述項目。
    /// </summary>
    /// <value>此分析器可回報的描述項；是否啟用及有效嚴重性由本次設定決定。</value>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(RuleDescriptors.EventSourcedAggregateDirectMutation);

    /// <summary>
    /// 初始化分析器並註冊指定指派與遞增遞減語法的分析。
    /// </summary>
    /// <param name="context">Roslyn 提供的註冊環境；由編譯器管理分析與取消生命週期。</param>
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
        var declaration = target.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (declaration is null)
        {
            return;
        }

        var aggregateSymbol = context.ContainingSymbol?.ContainingType;
        var classifier = RoleClassifier.For(context.Compilation, context.Options);
        if (aggregateSymbol is null || !classifier.Is(aggregateSymbol, "event-sourced-aggregate"))
        {
            return;
        }

        // A lambda/local function is a separate execution boundary even when declared inside When.
        var separateExecution = target.Ancestors().TakeWhile(node => node != declaration)
            .Any(node => node is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax);
        if (!separateExecution && context.ContainingSymbol is IMethodSymbol method && IsTransition(method, classifier)) return;

        var operation = context.SemanticModel.GetOperation(target, context.CancellationToken);
        var member = operation switch
        {
            IFieldReferenceOperation field => (ISymbol)field.Field,
            IPropertyReferenceOperation property => property.Property,
            _ => null
        };
        var receiver = operation switch
        {
            IFieldReferenceOperation field => field.Instance,
            IPropertyReferenceOperation property => property.Instance,
            _ => null
        };
        if (member is null || receiver is not IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance }
            || member.ContainingType is null
            || !IsAggregateMember(aggregateSymbol, member.ContainingType))
        {
            return;
        }

        DiagnosticReporting.Report(context,
            RuleDescriptors.EventSourcedAggregateDirectMutation,
            target.GetLocation(),
            aggregateSymbol.Name,
            member.Name);
    }

    /// <summary>只承認內建或精確映射 ES 根所宣告的 canonical transition 及其覆寫鏈。</summary>
    private static bool IsTransition(IMethodSymbol method, RoleClassifier classifier)
    {
        if (method.MethodKind != MethodKind.Ordinary || method.IsStatic || method.Name != "When"
            || !method.ReturnsVoid || method.Parameters.Length != 1
            || method.Parameters[0].RefKind != RefKind.None) return false;
        for (var current = method; current is not null; current = current.OverriddenMethod)
        {
            if (!(current.IsAbstract || current.IsVirtual || current.IsOverride)) continue;
            var owner = current.ContainingType;
            if (classifier.IsDirectContract(owner, "event-sourced-aggregate")
                && classifier.Match(owner, "event-sourced-aggregate") is { Matched: true, Confidence: "high" })
                return true;
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
