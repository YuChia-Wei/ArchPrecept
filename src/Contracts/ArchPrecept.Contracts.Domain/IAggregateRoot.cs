namespace ArchPrecept.Contracts.Domain;

/// <summary>
/// 表示聚合根的介面
/// 聚合根是領域模型中的一個概念，它代表一個由多個相關實體組成的集合，並負責確保這些實體的一致性
/// </summary>
/// <typeparam name="TId">聚合根識別碼的類型</typeparam>
/// <remarks>標記 Aggregate 邊界，不提供交易、持久化或跨實例一致性實作。</remarks>
public interface IAggregateRoot<out TId>
    : IDomainEntity<TId> where TId : notnull
{
    /// <summary>
    /// 聚合根的版本，用於處理高併發控制
    /// </summary>
    /// <value>實作所維護的版本；其遞增及儲存比較策略由實作與 adapter 決定。</value>
    int Version { get; }
}
