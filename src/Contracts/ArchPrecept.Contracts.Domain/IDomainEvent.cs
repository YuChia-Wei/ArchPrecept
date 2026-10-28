namespace ArchPrecept.Contracts.Domain;

/// <summary>
/// 表示領域事件的介面
/// 領域事件用於捕獲領域中發生的重要事情
/// </summary>
/// <remarks>事件實作應保存已發生的事實；此介面不強制深層不可變、產生識別碼或執行去重。</remarks>
public interface IDomainEvent
{
    /// <summary>
    /// 事件的唯一識別碼，用於冪等性處理
    /// </summary>
    /// <value>由事件建立者提供的穩定識別碼；消費者須自行實作冪等處理。</value>
    Guid EventId { get; }

    /// <summary>
    /// 事件發生的時間
    /// </summary>
    /// <value>由建立者決定的事件時間；本介面不強制 DateTime.Kind 或提供時鐘。</value>
    DateTime OccurredOn { get; }
}
