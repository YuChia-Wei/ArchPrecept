using ArchPrecept.Contracts.Application.Messaging;
using ArchPrecept.Contracts.Messaging;

var message = new OrderConfirmed("order-1", DateTime.UtcNow);
var delivery = new IntegrationMessageDelivery(Guid.NewGuid(), "customer-1");

// Existing adapters implementing only the original overload still receive the event.
var legacy = new LegacyPublisher();
IIntegrationEventPublisher legacyPort = legacy;
await legacyPort.PublishAsync(message, delivery);
if (!ReferenceEquals(legacy.LastEvent, message) || legacy.Calls != 1)
    throw new Exception("Default overload delegation changed.");

// A metadata-aware adapter explicitly implements the second overload.
var recorder = new RecordingPublisher();
IIntegrationEventPublisher port = recorder;
await port.PublishAsync(message, delivery);
if (!ReferenceEquals(recorder.LastEvent, message) || recorder.LastDelivery != delivery)
    throw new Exception("Delivery metadata was not preserved by the adapter.");
await port.PublishAsync(message);
if (recorder.LastDelivery is not null || recorder.Calls != 2)
    throw new Exception("Single-argument publish behavior changed.");

IIntegrationEventPublisher failing = new FailingPublisher();
try
{
    await failing.PublishAsync(message, delivery);
    throw new Exception("Publisher failure was swallowed.");
}
catch (IOException) { }
Console.WriteLine("Publisher overloads, metadata and failure propagation passed.");

public sealed record OrderConfirmed(string OrderId, DateTime OccurredOn) : IIntegrationEvent;

public sealed class LegacyPublisher : IIntegrationEventPublisher
{
    public IIntegrationEvent? LastEvent { get; private set; }
    public int Calls { get; private set; }
    public Task PublishAsync(IIntegrationEvent integrationEvent)
    {
        LastEvent = integrationEvent;
        Calls++;
        return Task.CompletedTask;
    }
}

public sealed class RecordingPublisher : IIntegrationEventPublisher
{
    public IIntegrationEvent? LastEvent { get; private set; }
    public IntegrationMessageDelivery? LastDelivery { get; private set; }
    public int Calls { get; private set; }
    public Task PublishAsync(IIntegrationEvent integrationEvent) => Record(integrationEvent, null);
    public Task PublishAsync(IIntegrationEvent integrationEvent, IntegrationMessageDelivery delivery) => Record(integrationEvent, delivery);
    private Task Record(IIntegrationEvent integrationEvent, IntegrationMessageDelivery? delivery)
    {
        LastEvent = integrationEvent;
        LastDelivery = delivery;
        Calls++;
        return Task.CompletedTask;
    }
}

public sealed class FailingPublisher : IIntegrationEventPublisher
{
    public Task PublishAsync(IIntegrationEvent integrationEvent) =>
        Task.FromException(new IOException("Synthetic adapter failure."));
}
