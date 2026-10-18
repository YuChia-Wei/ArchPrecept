using Microsoft.Extensions.Configuration;

namespace ArchPrecept.Contracts.Messaging.Abstractions.Configuration;

/// <summary>
/// 定義組合根可選擇的訊息傳輸設定檔。
/// </summary>
public enum MessagingTransportProfile
{
    /// <summary>
    /// 使用程序內記憶體傳輸。
    /// </summary>
    InMemory,

    /// <summary>
    /// 使用 Apache Kafka 傳輸。
    /// </summary>
    Kafka,

    /// <summary>
    /// 使用 RabbitMQ 傳輸。
    /// </summary>
    RabbitMq
}

/// <summary>
/// 提供經驗證的訊息代理選擇與連線設定契約。
/// </summary>
public sealed class MessagingTransportOptions
{
    /// <summary>
    /// 取得設定區段名稱。
    /// </summary>
    public const string SectionName = "Messaging";

    /// <summary>
    /// 取得選定的訊息傳輸設定檔。
    /// </summary>
    public MessagingTransportProfile Profile { get; private init; }

    /// <summary>
    /// 取得 Kafka 設定檔所需的連線字串。
    /// </summary>
    public string? KafkaConnectionString { get; private init; }

    /// <summary>
    /// 取得 RabbitMQ 設定檔所需的連線字串。
    /// </summary>
    public string? RabbitMqConnectionString { get; private init; }

    /// <summary>
    /// 從設定來源讀取、建立並驗證訊息傳輸選項。
    /// </summary>
    /// <param name="configuration">提供訊息傳輸設定的組態來源。</param>
    /// <returns>已通過驗證的訊息傳輸選項。</returns>
    public static MessagingTransportOptions FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetRequiredSection(SectionName);
        var profileValue = section[nameof(Profile)];
        if (!Enum.TryParse<MessagingTransportProfile>(profileValue, true, out var profile) || !Enum.IsDefined(profile))
        {
            throw new InvalidOperationException(
                $"{SectionName}:{nameof(Profile)} must be one of: InMemory, Kafka, RabbitMq.");
        }

        var options = new MessagingTransportOptions
        {
            Profile = profile,
            KafkaConnectionString = section["Kafka:ConnectionString"],
            RabbitMqConnectionString = section["RabbitMq:ConnectionString"]
        };

        options.Validate();
        return options;
    }

    /// <summary>
    /// 取得已驗證的 Kafka 連線字串。
    /// </summary>
    /// <returns>Kafka 連線字串。</returns>
    public string GetRequiredKafkaConnectionString()
    {
        return KafkaConnectionString!;
    }

    /// <summary>
    /// 取得已驗證的 RabbitMQ 絕對 URI。
    /// </summary>
    /// <returns>RabbitMQ 的 AMQP 或 AMQPS URI。</returns>
    public Uri GetRequiredRabbitMqUri()
    {
        return new Uri(RabbitMqConnectionString!, UriKind.Absolute);
    }

    /// <summary>
    /// 驗證目前設定檔所需的連線設定與 RabbitMQ URI 格式。
    /// </summary>
    private void Validate()
    {
        if (Profile == MessagingTransportProfile.Kafka && string.IsNullOrWhiteSpace(KafkaConnectionString))
        {
            throw new InvalidOperationException(
                $"{SectionName}:Kafka:ConnectionString is required for the Kafka profile.");
        }

        if (Profile != MessagingTransportProfile.RabbitMq)
        {
            return;
        }

        if (!Uri.TryCreate(RabbitMqConnectionString, UriKind.Absolute, out var uri) || (uri.Scheme != "amqp" && uri.Scheme != "amqps"))
        {
            throw new InvalidOperationException(
                $"{SectionName}:RabbitMq:ConnectionString must be an absolute amqp or amqps URI for the RabbitMq profile.");
        }
    }
}
