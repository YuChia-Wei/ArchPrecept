namespace ArchPrecept.Contracts.Application;

/// <summary>
/// 表示分頁查詢結果。
/// </summary>
/// <typeparam name="TItem">分頁項目型別。</typeparam>
/// <remarks>保留呼叫端提供的清單參考，不複製項目或驗證分頁數值。頁碼起算方式由 query port 定義；
/// IReadOnlyList 不保證底層集合或項目不可變，呼叫端須維持所需的 snapshot 語意。</remarks>
public sealed class PageResult<TItem>
{
    /// <summary>
    /// 初始化分頁查詢結果。
    /// </summary>
    /// <param name="items">當前頁面的資料集合。</param>
    /// <param name="totalCount">符合條件的總筆數。</param>
    /// <param name="pageIndex">頁碼索引。</param>
    /// <param name="pageSize">每頁筆數。</param>
    /// <remarks>直接保存參數，未執行範圍或一致性驗證；query adapter 應先產生符合其契約的資料。</remarks>
    public PageResult(IReadOnlyList<TItem> items, int totalCount, int pageIndex, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }

    /// <summary>
    /// 取得當前頁面的資料集合。
    /// </summary>
    /// <value>建構時提供的清單參考。</value>
    public IReadOnlyList<TItem> Items { get; }

    /// <summary>
    /// 取得符合條件的總筆數。
    /// </summary>
    /// <value>呼叫端提供的總筆數，不由 Items.Count 推算。</value>
    public int TotalCount { get; }

    /// <summary>
    /// 取得頁碼索引。
    /// </summary>
    /// <value>呼叫端提供的索引；起算方式由 query 契約說明。</value>
    public int PageIndex { get; }

    /// <summary>
    /// 取得每頁筆數。
    /// </summary>
    /// <value>呼叫端提供的分頁大小，不限制 Items.Count。</value>
    public int PageSize { get; }
}
