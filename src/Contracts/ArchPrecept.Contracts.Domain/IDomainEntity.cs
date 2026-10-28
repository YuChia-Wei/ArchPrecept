namespace ArchPrecept.Contracts.Domain;

/// <summary>
/// 表示領域實體的介面
/// 領域實體是具有唯一識別碼的物件
/// </summary>
/// <typeparam name="TId">實體識別碼的類型</typeparam>
/// <remarks>實作擁有 identity 及事件生命週期。本介面不規定事件傳遞保證；ES 實作可拒絕一般事件 mutator。</remarks>
public interface IDomainEntity<out TId> where TId : notnull
{
    /// <summary>
    /// 取得實體的識別碼
    /// </summary>
    /// <value>實體已建立的非 null identity；未完成重建的實作可拒絕讀取。</value>
    TId Id { get; }

    /// <summary>
    /// 取得實體的領域事件
    /// </summary>
    /// <value>實作提供的唯讀事件集合；不保證為快照或事件物件深層不可變。</value>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// 添加領域事件
    /// </summary>
    /// <param name="domainEvent">要添加的領域事件</param>
    /// <remarks>不等於發布事件。ES Aggregate 的實作拒絕此操作，要求以事件轉換狀態。</remarks>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// 清除所有領域事件
    /// </summary>
    /// <remarks>清除不等於傳遞或提交成功；ES Aggregate 須改用專屬提交確認操作。</remarks>
    void ClearDomainEvents();

    /// <summary>
    /// 移除領域事件
    /// </summary>
    /// <param name="domainEvent">要移除的領域事件</param>
    /// <remarks>是否允許移除及相等性策略由實作決定；ES Aggregate 拒絕移除已套用事件。</remarks>
    void RemoveDomainEvent(IDomainEvent domainEvent);
}
