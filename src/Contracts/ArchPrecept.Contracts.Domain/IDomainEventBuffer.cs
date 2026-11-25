namespace ArchPrecept.Contracts.Domain;

/// <summary>管理一般領域實體的本機事件集合。呼叫端負責實際的傳遞與持久化。</summary>
public interface IDomainEventBuffer : IDomainEventSource
{
    /// <summary>將事件加入集合尾端，保留記錄順序與重複項目。</summary>
    /// <param name="domainEvent">要記錄的事件。</param>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>依相等性移除第一個相符事件；找不到時保留集合。</summary>
    /// <param name="domainEvent">要移除的事件。</param>
    void RemoveDomainEvent(IDomainEvent domainEvent);

    /// <summary>清空本機事件集合；由呼叫端決定何時處理完畢。</summary>
    void ClearDomainEvents();
}
