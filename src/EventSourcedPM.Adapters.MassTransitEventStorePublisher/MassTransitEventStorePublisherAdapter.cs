using EventSourcedPM.Ports.EventStore;
using MassTransit;

namespace EventSourcedPM.Adapters.MassTransitEventStorePublisher;

public class MassTransitEventStorePublisherAdapter(IBus bus) : IEventPublisher
{
    private IBus Bus { get; } = bus;

    public async Task Publish(IEnumerable<EventWithMetadata> events, CancellationToken cancellationToken = default)
    {
        foreach (var evt in events)
        {
            await Bus.Publish(
                evt.Event,
                evt.Event.GetType(),
                context =>
                {
                    context.MessageId = evt.Metadata.EventId;
                    context.CorrelationId = evt.Metadata.CorrelationId;
                    context.RequestId = evt.Metadata.CausationId;
                },
                cancellationToken
            );
        }
    }
}
