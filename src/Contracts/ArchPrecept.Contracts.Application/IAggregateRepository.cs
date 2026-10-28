using ArchPrecept.Contracts.Domain;

namespace ArchPrecept.Contracts.Application;

/// <summary>
/// 定義載入與保存單一聚合根的標準寫入端連接埠。
/// </summary>
/// <typeparam name="TAggregate">由此 port 載入及保存的 Aggregate Root 型別。</typeparam>
/// <typeparam name="TId">該 Aggregate 的 identity 型別。</typeparam>
/// <remarks>此最小 write port 僅依 identity 載入與保存，criteria／排序／分頁／DTO 查詢應另設 query port。
/// Save 表達保存意圖，不指定 insert、update 或 append；交易、並行控制及事件提交由 adapter 定義。</remarks>
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
    /// <remarks>回傳可供命令處理的 Aggregate；呼叫者仍須依目前狀態驗證 invariant。取消及儲存錯誤由 adapter 傳遞，介面不保證快取或追蹤模式。</remarks>
    Task<TAggregate?> FindByIdAsync(
        TId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 非同步保存聚合根的狀態變更。
    /// </summary>
    /// <param name="aggregate">要保存的聚合根。</param>
    /// <param name="cancellationToken">取消非同步作業的權杖。</param>
    /// <returns>代表非同步保存作業的工作。</returns>
    /// <remarks>不預設多個 Aggregate 的原子性、事件已發布或 pending 已清除。取消不表示儲存必定未發生，adapter 須說明完成界線。</remarks>
    Task SaveAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 提供既有聚合儲存庫可使用的相容性別名。
/// </summary>
/// <typeparam name="TAggregate">此 port 的 Aggregate Root 型別。</typeparam>
/// <typeparam name="TId">Aggregate 的 identity 型別。</typeparam>
/// <remarks>與 IAggregateRepository 相同的最小載入／保存契約，不增加 Entity CRUD 或查詢能力。</remarks>
public interface IDomainRepository<TAggregate, in TId>
    : IAggregateRepository<TAggregate, TId>
    where TAggregate : IAggregateRoot<TId>
    where TId : notnull
{
}
