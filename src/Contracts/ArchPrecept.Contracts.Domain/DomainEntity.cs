namespace ArchPrecept.Contracts.Domain;

/// <summary>
/// 提供具識別碼與領域事件集合的領域實體基底實作。
/// </summary>
/// <typeparam name="TId">實體識別碼的型別。</typeparam>
/// <remarks>識別碼由領域建構子或重建程序指派。本基底只拒絕 null，不判斷零值、空字串或識別碼的業務有效性。
/// 實例及其事件集合不支援並行修改；本基底不提供持久化、事件傳遞或交易保證。</remarks>
public abstract class DomainEntity<TId>
    : IDomainEntity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();
    private TId? _identityValue;
    private bool _hasIdentity;

    /// <summary>
    /// 以指定識別碼初始化領域實體。
    /// </summary>
    /// <param name="id">實體識別碼。</param>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> 為 null。</exception>
    protected DomainEntity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// 初始化未指派識別碼的領域實體，供 ORM 或序列化框架使用。
    /// </summary>
    /// <remarks>重建程序須經由 Id setter 指派識別碼，再將實例交給業務程式。
    /// ORM 可使用參數化建構子或屬性存取；直接寫入私有欄位不屬於支援的重建契約。</remarks>
    protected DomainEntity()
    {
    }

    /// <summary>
    /// 取得或設定實體識別碼。
    /// </summary>
    /// <value>已明確指派且非 null 的識別碼；值型別的 default 值不代表尚未指派。</value>
    /// <exception cref="InvalidOperationException">尚未指派識別碼便讀取此屬性。</exception>
    /// <exception cref="ArgumentNullException">嘗試指派 null。</exception>
    /// <remarks>protected setter 支援重建及建立事件。衍生領域模型負責確保 identity 的連續性，
    /// 不應將此 setter 當成可任意更換實體身分的公開操作。</remarks>
    public TId Id
    {
        get => _hasIdentity && _identityValue is not null
            ? _identityValue
            : throw new InvalidOperationException("Entity identity has not been assigned.");
        protected set
        {
            ArgumentNullException.ThrowIfNull(value);
            _identityValue = value;
            _hasIdentity = true;
        }
    }

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
    public virtual void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// 從待處理集合移除指定領域事件。
    /// </summary>
    /// <param name="domainEvent">要移除的領域事件。</param>
    /// <remarks>依預設相等性移除第一個相符項目；不存在時不變更集合，不撤銷事件的業務效果。</remarks>
    public virtual void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// 清除所有待處理的領域事件。
    /// </summary>
    /// <remarks>只清除此實例的事件集合；呼叫端負責決定何時已完成事件處理。</remarks>
    public virtual void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
