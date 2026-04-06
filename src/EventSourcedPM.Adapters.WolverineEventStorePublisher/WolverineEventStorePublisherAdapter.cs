using EventSourcedPM.Ports.EventStore;
using Wolverine;
using IWolverineBus = Wolverine.IMessageBus;

namespace EventSourcedPM.Adapters.WolverineEventStorePublisher;

public class WolverineEventStorePublisherAdapter(IWolverineBus bus) : IEventPublisher
{
    public async Task Publish(IEnumerable<EventWithMetadata> events, CancellationToken cancellationToken = default)
    {
        foreach (var evt in events)
        {
            var options = new DeliveryOptions();
            options.Headers["correlation-id"] = evt.Metadata.CorrelationId.ToString();
            options.Headers["message-id"] = evt.Metadata.EventId.ToString();
            if (evt.Metadata.CausationId.HasValue)
                options.Headers["causation-id"] = evt.Metadata.CausationId.Value.ToString();

            await bus.PublishAsync(evt.Event, options);
        }
    }
}
