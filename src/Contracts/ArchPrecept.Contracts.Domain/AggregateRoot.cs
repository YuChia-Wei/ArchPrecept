namespace ArchPrecept.Contracts.Domain;

/// <summary>
/// 提供一般領域聚合根的基底實作。
/// </summary>
/// <typeparam name="TId">聚合根識別碼的型別。</typeparam>
/// <remarks>衍生模型負責 Aggregate 不變條件；版本數值本身不執行 optimistic concurrency 或儲存交易。</remarks>
public abstract class AggregateRoot<TId>
    : DomainEntity<TId>, IAggregateRoot<TId> where TId : notnull
{
    /// <summary>
    /// 以指定識別碼初始化聚合根。
    /// </summary>
    /// <param name="id">聚合根識別碼。</param>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> 為 null。</exception>
    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// 初始化未指派識別碼的聚合根，供 ORM 或序列化框架使用。
    /// </summary>
    /// <remarks>重建完成前須經由 Id setter 指派識別碼；提早讀取 Id 會擲回 InvalidOperationException。</remarks>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// 取得或設定聚合根版本，以支援並行控制。
    /// </summary>
    /// <value>領域模型或 adapter 維護的版本，初始為零。</value>
    /// <remarks>一般 Aggregate 不在基底自動遞增版本；儲存端須自行驗證預期版本。</remarks>
    public int Version { get; protected set; }
}
