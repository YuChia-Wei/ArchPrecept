namespace ArchPrecept.Contracts.Domain;

/// <summary>
/// 表示領域實體的介面
/// 領域實體是具有唯一識別碼的物件
/// </summary>
/// <typeparam name="TId">實體識別碼的類型</typeparam>
/// <remarks>只需識別實體的呼叫端可使用此介面。</remarks>
public interface IDomainEntity<out TId> where TId : notnull
{
    /// <summary>
    /// 取得實體的識別碼
    /// </summary>
    /// <value>實體已建立的非 null identity；未完成重建的實作可拒絕讀取。</value>
    TId Id { get; }
}
