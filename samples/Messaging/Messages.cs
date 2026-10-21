using System.Text.Json;
using ArchPrecept.Contracts.Messaging;

// A receiver can use the message contracts without the publisher port or Domain.
var occurredOn = new DateTime(2026, 10, 21, 0, 0, 0, DateTimeKind.Utc);
var original = new OrderConfirmed("order-1", occurredOn);
var restored = JsonSerializer.Deserialize<OrderConfirmed>(JsonSerializer.Serialize(original))
    ?? throw new Exception("Integration event was null.");
if (restored != original) throw new Exception("Integration event JSON round trip failed.");
IIntegrationEvent integrationEvent = restored;
if (integrationEvent.OccurredOn != occurredOn) throw new Exception("Event timestamp changed.");

var request = new FindOrder("order-1");
IRequestContract requestContract = request;
if (!Equals(JsonSerializer.Deserialize<FindOrder>(JsonSerializer.Serialize(request)), requestContract))
    throw new Exception("Request message JSON round trip failed.");
Console.WriteLine("Standalone message contracts and JSON round trips passed.");

public sealed record OrderConfirmed(string OrderId, DateTime OccurredOn) : IIntegrationEvent;
public sealed record FindOrder(string OrderId) : IRequestContract;
