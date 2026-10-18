namespace ArchPrecept.Contracts.Domain;

/// <summary>
/// 值物件 (Value Object) 介面
/// 值物件是用來描述領域內某個事物特徵的不可變物件
/// 它們沒有身份識別，而是透過其屬性值來判斷等同性
/// 實作此介面的類型必須：
/// 1. 確保不可變性 (Immutability)
/// 2. 實作適當的等同性比較 (通常使用 C# record 即可)
/// 3. 不應包含任何領域實體參考，只引用其他值物件或基本型別
/// </summary>
public interface IValueObject
{
    // 標記介面，無方法定義
    // 主要用於識別值物件及啟用靜態程式碼分析
}

/// <summary>
/// 提供以相等性元件判斷值物件相等性的基底實作。
/// </summary>
public abstract class ValueObject : IValueObject
{
    /// <summary>
    /// 比較目前值物件與另一個物件的型別及相等性元件是否一致。
    /// </summary>
    /// <param name="obj">要比較的物件。</param>
    /// <returns>兩個物件為相同值物件時為 <see langword="true" />；否則為 <see langword="false" />。</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// 取得用於比較值物件相等性的有序元件。
    /// </summary>
    /// <returns>用於相等性比較的元件序列。</returns>
    public abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// 依相等性元件計算值物件的雜湊碼。
    /// </summary>
    /// <returns>值物件的雜湊碼。</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
               .Select(x => x.GetHashCode())
               .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// 比較兩個值物件是否相等。
    /// </summary>
    /// <param name="a">第一個值物件。</param>
    /// <param name="b">第二個值物件。</param>
    /// <returns>兩個值物件相等時為 <see langword="true" />；否則為 <see langword="false" />。</returns>
    public static bool operator ==(ValueObject a, ValueObject b)
    {
        if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
        {
            return true;
        }

        if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
        {
            return false;
        }

        return a.Equals(b);
    }

    /// <summary>
    /// 比較兩個值物件是否不相等。
    /// </summary>
    /// <param name="a">第一個值物件。</param>
    /// <param name="b">第二個值物件。</param>
    /// <returns>兩個值物件不相等時為 <see langword="true" />；否則為 <see langword="false" />。</returns>
    public static bool operator !=(ValueObject a, ValueObject b)
    {
        return !(a == b);
    }
}
