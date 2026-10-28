namespace ArchPrecept.Contracts.Application;

/// <summary>
/// 表示不含值的結果模式回傳物件。
/// </summary>
/// <remarks>表示應用操作的邏輯結果，不代表交易已提交；錯誤文字不被驗證或轉換成例外。</remarks>
public sealed class Result
{
    /// <summary>
    /// 以指定成功狀態與錯誤訊息初始化結果。
    /// </summary>
    private Result(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// 取得是否成功。
    /// </summary>
    /// <value>由 Success 或 Failure 建立時固定的邏輯狀態。</value>
    public bool IsSuccess { get; }

    /// <summary>
    /// 取得失敗時的錯誤訊息。
    /// </summary>
    /// <value>成功時為 null；失敗時為呼叫端提供的文字。</value>
    public string? ErrorMessage { get; }

    /// <summary>
    /// 建立失敗結果。
    /// </summary>
    /// <param name="errorMessage">錯誤訊息。</param>
    /// <returns>失敗結果。</returns>
    public static Result Failure(string errorMessage)
    {
        return new Result(false, errorMessage);
    }

    /// <summary>
    /// 建立成功結果。
    /// </summary>
    /// <returns>成功結果。</returns>
    public static Result Success()
    {
        return new Result(true, null);
    }
}

/// <summary>
/// 表示含有值的結果模式回傳物件。
/// </summary>
/// <typeparam name="TValue">成功時承載的值型別。</typeparam>
/// <remarks>不複製或凍結 Value；成功可承載 null 或 default 值，失敗也使用 default。
/// 應以 IsSuccess 區分成功與失敗，不以 Value 是否為 null 判斷。</remarks>
public sealed class Result<TValue>
{
    /// <summary>
    /// 以指定成功狀態、值與錯誤訊息初始化結果。
    /// </summary>
    private Result(bool isSuccess, TValue? value, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// 取得是否成功。
    /// </summary>
    /// <value>由 Success 或 Failure 建立時固定的邏輯狀態。</value>
    public bool IsSuccess { get; }

    /// <summary>
    /// 取得成功時承載的值。
    /// </summary>
    /// <value>成功時為原傳入值；失敗時為 default，值型別不一定為 null。</value>
    public TValue? Value { get; }

    /// <summary>
    /// 取得失敗時的錯誤訊息。
    /// </summary>
    /// <value>成功時為 null；失敗時為原傳入文字。</value>
    public string? ErrorMessage { get; }

    /// <summary>
    /// 建立失敗結果。
    /// </summary>
    /// <param name="errorMessage">錯誤訊息。</param>
    /// <returns>失敗結果。</returns>
    public static Result<TValue> Failure(string errorMessage)
    {
        return new Result<TValue>(false, default, errorMessage);
    }

    /// <summary>
    /// 建立成功結果。
    /// </summary>
    /// <param name="value">成功時承載的值。</param>
    /// <returns>成功結果。</returns>
    public static Result<TValue> Success(TValue value)
    {
        return new Result<TValue>(true, value, null);
    }
}
