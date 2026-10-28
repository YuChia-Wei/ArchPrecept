namespace ArchPrecept.Contracts.Application;

/// <summary>
/// 標記 Application 輸出端連接埠為唯讀查詢存取。
/// </summary>
/// <remarks>衍生 port 定義 DTO／read-model 查詢，不應暴露寫入或回傳可變 Entity／Aggregate。
/// marker 與靜態方法形狀不證明查詢沒有副作用，也不提供 snapshot 或交易隔離。</remarks>
public interface IQueryRepository
{
}
