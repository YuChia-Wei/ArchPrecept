namespace ArchPrecept.Contracts.Domain;

/// <summary>
/// 提供一般領域聚合根的基底實作。
/// </summary>
/// <typeparam name="TId">聚合根識別碼的型別。</typeparam>
public abstract class AggregateRoot<TId>
    : DomainEntity<TId>, IAggregateRoot<TId> where TId : notnull
{
    /// <summary>
    /// 以指定識別碼初始化聚合根。
    /// </summary>
    /// <param name="id">聚合根識別碼。</param>
    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// 初始化未指派識別碼的聚合根，供 ORM 或序列化框架使用。
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// 取得或設定聚合根版本，以支援並行控制。
    /// </summary>
    public int Version { get; protected set; }
}
