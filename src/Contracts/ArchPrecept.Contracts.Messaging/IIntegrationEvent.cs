namespace ArchPrecept.Contracts.Messaging;

/// <summary>
/// 表示跨系統或有界上下文交換的整合事件契約，不包含發布或傳輸設定。
/// </summary>
/// <remarks>
/// 擁有者應提供可穩定序列化的事實資料；此介面不強制不可變性，不提供 broker、重試、去重或事件傳遞保證。
/// </remarks>
public interface IIntegrationEvent
{
    /// <summary>
    /// 發生時間
    /// </summary>
    /// <value>事件建立者提供的發生時間；時區與序列化格式由交換契約決定。</value>
    DateTime OccurredOn { get; }
}
