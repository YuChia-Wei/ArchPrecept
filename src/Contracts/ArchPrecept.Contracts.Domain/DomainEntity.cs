namespace ArchPrecept.Contracts.Domain;

/// <summary>
/// 提供具識別碼與領域事件集合的領域實體基底實作。
/// </summary>
/// <typeparam name="TId">實體識別碼的型別。</typeparam>
public abstract class DomainEntity<TId>
    : IDomainEntity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// 以指定識別碼初始化領域實體。
    /// </summary>
    /// <param name="id">實體識別碼。</param>
    protected DomainEntity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// 初始化未指派識別碼的領域實體，供 ORM 或序列化框架使用。
    /// </summary>
    protected DomainEntity()
    {
    }

    /// <summary>
    /// 取得或設定實體識別碼。
    /// </summary>
    public TId Id { get; protected set; }

    /// <summary>
    /// 取得尚待處理的領域事件唯讀集合。
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// 將領域事件加入待處理集合。
    /// </summary>
    /// <param name="domainEvent">要加入的領域事件。</param>
    public virtual void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// 從待處理集合移除指定領域事件。
    /// </summary>
    /// <param name="domainEvent">要移除的領域事件。</param>
    public virtual void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// 清除所有待處理的領域事件。
    /// </summary>
    public virtual void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
