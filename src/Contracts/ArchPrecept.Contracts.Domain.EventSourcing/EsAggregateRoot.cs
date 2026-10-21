using ArchPrecept.Contracts.Domain;

namespace ArchPrecept.Contracts.Domain.EventSourcing;

/// <summary>
/// 提供事件重播、待提交事件與提交確認的聚合根基底。
/// </summary>
/// <remarks>
/// Version 表示已重播或確認提交的事件數，空事件流為 0；新事件不立即增加版本。
/// 儲存端必須自行以 expected revision 原子附加事件，成功後才呼叫 MarkChangesAsCommitted。
/// 本型別不支援並行操作；附加期間不得修改同一實例。When 必須只轉換狀態，
/// 不得執行外部副作用或再次套用事件。轉換或重播失敗後必須丟棄實例並重新載入。
/// </remarks>
/// <typeparam name="TId">聚合根識別碼的型別。</typeparam>
public abstract class EsAggregateRoot<TId> : AggregateRoot<TId> where TId : notnull
{
    private bool _hasStarted;
    private bool _isTransitioning;
    private bool _isFaulted;

    /// <summary>
    /// 初始化指定識別碼的新聚合根；不在基底建構子呼叫事件轉換。
    /// </summary>
    protected EsAggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// 初始化尚未指派識別碼的聚合根；識別碼可由建立事件設定。
    /// </summary>
    protected EsAggregateRoot()
    {
    }

    /// <summary>
    /// 在衍生類別初始化完成後，依事件流順序重播完整歷史；不記錄待提交事件。
    /// 每個實例只允許在任何新事件之前載入一次，不支援附加歷史或快照起始版本。
    /// </summary>
    /// <param name="events">從事件流起點依序提供的歷史事件。</param>
    protected void LoadFromHistory(IEnumerable<IDomainEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        EnsureCanTransition();
        if (_hasStarted)
        {
            throw new InvalidOperationException("History can only be loaded once, before applying new events.");
        }

        _hasStarted = true;
        _isTransitioning = true;
        try
        {
            foreach (var @event in events)
            {
                ArgumentNullException.ThrowIfNull(@event);
                var nextVersion = checked(Version + 1);
                When(@event);
                Version = nextVersion;
            }
        }
        catch
        {
            _isFaulted = true;
            throw;
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    /// <summary>
    /// 透過事件轉換狀態，成功後依序記錄待提交事件；已提交版本保持不變。
    /// </summary>
    protected void Apply(IDomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        EnsureCanTransition();
        _ = checked(Version + DomainEvents.Count + 1);
        _hasStarted = true;
        _isTransitioning = true;
        try
        {
            When(@event);
            base.AddDomainEvent(@event);
        }
        catch
        {
            _isFaulted = true;
            throw;
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    /// <summary>
    /// 在儲存端確認所有待提交事件已附加後，更新版本並清除待提交事件。
    /// 此方法只檢查本機版本，不執行持久化或儲存端的並行控制。
    /// </summary>
    /// <param name="committedVersion">附加成功後的事件總數，必須等於 Version 加上待提交事件數。</param>
    public void MarkChangesAsCommitted(int committedVersion)
    {
        EnsureCanTransition();
        var expectedVersion = checked(Version + DomainEvents.Count);
        if (committedVersion != expectedVersion)
        {
            throw new InvalidOperationException(
                $"Committed version {committedVersion} does not match expected version {expectedVersion}.");
        }

        Version = committedVersion;
        base.ClearDomainEvents();
    }

    /// <summary>
    /// 取得最後一個待提交事件；事件不存在時傳回 null。
    /// </summary>
    protected IDomainEvent? GetLastDomainEvent() => DomainEvents.LastOrDefault();

    /// <summary>
    /// 拒絕繞過 Apply 直接加入待提交事件，即使經由一般領域實體介面呼叫亦同。
    /// </summary>
    public sealed override void AddDomainEvent(IDomainEvent domainEvent) =>
        throw new NotSupportedException("Use Apply to change state and record an event together.");

    /// <summary>
    /// 拒絕移除已套用的事件，以維持狀態與待提交事件一致。
    /// </summary>
    public sealed override void RemoveDomainEvent(IDomainEvent domainEvent) =>
        throw new NotSupportedException("Applied events cannot be removed.");

    /// <summary>
    /// 拒絕直接清除待提交事件；只有成功提交確認可以清除。
    /// </summary>
    public sealed override void ClearDomainEvents() =>
        throw new NotSupportedException("Use MarkChangesAsCommitted after a successful append.");

    /// <summary>
    /// 檢查實例是否可開始下一次轉換，拒絕重入或在失敗實例上繼續處理。
    /// </summary>
    private void EnsureCanTransition()
    {
        if (_isFaulted || _isTransitioning)
        {
            throw new InvalidOperationException("The aggregate is faulted or already processing an event.");
        }
    }

    /// <summary>
    /// 根據事件轉換內部狀態；無法識別的事件應擲回例外，避免靜默略過歷史。
    /// </summary>
    protected abstract void When(IDomainEvent @event);
}
