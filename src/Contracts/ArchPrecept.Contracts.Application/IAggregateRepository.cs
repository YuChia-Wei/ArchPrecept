using ArchPrecept.Contracts.Domain;

namespace ArchPrecept.Contracts.Application;

/// <summary>
/// 定義載入與保存單一聚合根的標準寫入端連接埠。
/// </summary>
public interface IAggregateRepository<TAggregate, in TId>
    where TAggregate : IAggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// 依識別碼非同步取得聚合根。
    /// </summary>
    /// <param name="id">聚合根識別碼。</param>
    /// <param name="cancellationToken">取消非同步作業的權杖。</param>
    /// <returns>找到的聚合根；找不到時為 <see langword="null" />。</returns>
    Task<TAggregate?> FindByIdAsync(
        TId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 非同步保存聚合根的狀態變更。
    /// </summary>
    /// <param name="aggregate">要保存的聚合根。</param>
    /// <param name="cancellationToken">取消非同步作業的權杖。</param>
    /// <returns>代表非同步保存作業的工作。</returns>
    Task SaveAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 提供既有聚合儲存庫可使用的相容性別名。
/// </summary>
public interface IDomainRepository<TAggregate, in TId>
    : IAggregateRepository<TAggregate, TId>
    where TAggregate : IAggregateRoot<TId>
    where TId : notnull
{
}
