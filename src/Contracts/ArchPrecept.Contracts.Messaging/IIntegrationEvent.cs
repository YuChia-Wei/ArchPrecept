namespace ArchPrecept.Contracts.Messaging;

/// <summary>
/// 表示跨系統或有界上下文交換的整合事件契約，不包含發布或傳輸設定。
/// </summary>
/// <remarks>
/// ref:
/// https://medium.com/%40serhatalftkn/domain-events-vs-integration-events-understanding-the-differences-and-when-to-use-each-3977278034d3
/// </remarks>
public interface IIntegrationEvent
{
    /// <summary>
    /// 發生時間
    /// </summary>
    DateTime OccurredOn { get; }
}
