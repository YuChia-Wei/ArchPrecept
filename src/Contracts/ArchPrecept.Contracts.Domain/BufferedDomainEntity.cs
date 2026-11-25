namespace ArchPrecept.Contracts.Domain;

/// <summary>提供具有識別碼及可編輯領域事件集合的一般實體基底。</summary>
/// <typeparam name="TId">實體識別碼的型別。</typeparam>
/// <remarks>實例由單一呼叫端管理。傳遞、持久化及清除時機由應用程式決定。</remarks>
public abstract class BufferedDomainEntity<TId> : DomainEntity<TId>, IDomainEventBuffer where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>建立已指派 identity、尚無事件的實體。</summary>
    /// <param name="id">非 null 的領域識別碼。</param>
    protected BufferedDomainEntity(TId id) : base(id) { }

    /// <summary>建立尚未指派 identity 的實體，供重建程序指派後使用。</summary>
    protected BufferedDomainEntity() { }

    /// <summary>
    /// 取得尚待處理的領域事件唯讀集合。
    /// </summary>
    /// <value>反映目前集合的唯讀 view，保留事件加入順序。</value>
    /// <remarks>不是快照，也不複製事件物件；事件內容的不可變性由呼叫端保證。清除集合不代表事件已成功傳遞。</remarks>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// 將領域事件加入待處理集合。
    /// </summary>
    /// <param name="domainEvent">要加入的領域事件。</param>
    /// <remarks>保留傳入物件參考，允許重複事件；不發布事件或驗證其內容。</remarks>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// 從待處理集合移除指定領域事件。
    /// </summary>
    /// <param name="domainEvent">要移除的領域事件。</param>
    /// <remarks>依預設相等性移除第一個相符項目；不存在時不變更集合，不撤銷事件的業務效果。</remarks>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// 清除所有待處理的領域事件。
    /// </summary>
    /// <remarks>只清除此實例的事件集合；呼叫端負責決定何時已完成事件處理。</remarks>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
