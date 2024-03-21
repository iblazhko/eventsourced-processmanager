namespace EventSourcedPM.Adapters.MassTransitEventStorePublisher;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventSourcedPM.Ports.EventStore;
using MassTransit;

public class MassTransitEventStorePublisherAdapter(IBus bus) : IEventPublisher
{
    private IBus Bus { get; } = bus;

    public async Task Publish(
        IEnumerable<EventWithMetadata> events,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var evt in events)
        {
            await Bus.Publish(evt.Event, evt.Event.GetType(), cancellationToken);
        }
    }
}
