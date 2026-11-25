namespace ArchPrecept.Contracts.Domain;

/// <summary>提供待處理領域事件的唯讀檢視，供讀取或複製當批事件。</summary>
public interface IDomainEventSource
{
    /// <summary>依記錄順序取得目前待處理的事件。</summary>
    /// <value>事件物件的原參考；集合內容可隨所屬實例的操作更新。</value>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
}
