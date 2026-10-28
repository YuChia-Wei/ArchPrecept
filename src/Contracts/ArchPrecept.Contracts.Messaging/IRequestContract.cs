namespace ArchPrecept.Contracts.Messaging;

/// <summary>
/// 標記可跨系統交換的請求訊息，不代表程序內的使用案例或 Command Handler。
/// </summary>
/// <remarks>只標記訊息責任，不定義回應、correlation、取消、序列化或可靠交付機制；各 adapter 另行約定。</remarks>
public interface IRequestContract
{
}
