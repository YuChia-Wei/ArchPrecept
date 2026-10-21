using Microsoft.Extensions.Configuration;
using MessagingSample.Host;

// Host-owned settings; this example does not connect to a broker.
var inMemory = Load("InMemory");
if (inMemory.Profile != MessagingTransportProfile.InMemory)
    throw new Exception("InMemory profile changed.");

var kafka = Load("Kafka", kafka: "localhost:9092");
if (kafka.Profile != MessagingTransportProfile.Kafka || kafka.GetRequiredKafkaConnectionString() != "localhost:9092")
    throw new Exception("Kafka configuration changed.");

foreach (var address in new[] { "amqp://localhost:5672", "amqps://localhost:5671" })
{
    var rabbit = Load("RabbitMq", rabbit: address);
    if (rabbit.Profile != MessagingTransportProfile.RabbitMq || rabbit.GetRequiredRabbitMqUri() != new Uri(address))
        throw new Exception("RabbitMQ configuration changed.");
}

ExpectInvalid(() => Load("unknown"));
ExpectInvalid(() => Load("999"));
ExpectInvalid(() => Load("Kafka", kafka: " "));
ExpectInvalid(() => Load("RabbitMq"));
ExpectInvalid(() => Load("RabbitMq", rabbit: "https://localhost"));
ExpectInvalid(() => MessagingTransportOptions.FromConfiguration(new ConfigurationBuilder().Build()));
Console.WriteLine("Host-owned messaging configuration valid/invalid cases passed; no broker connection attempted.");

static MessagingTransportOptions Load(string profile, string? kafka = null, string? rabbit = null)
{
    var values = new Dictionary<string, string?>
    {
        ["Messaging:Profile"] = profile,
        ["Messaging:Kafka:ConnectionString"] = kafka,
        ["Messaging:RabbitMq:ConnectionString"] = rabbit
    };
    return MessagingTransportOptions.FromConfiguration(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
}

static void ExpectInvalid(Action action)
{
    try { action(); }
    catch (InvalidOperationException) { return; }
    throw new Exception("Invalid host configuration was accepted.");
}
