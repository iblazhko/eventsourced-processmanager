namespace EventSourcedPM.Adapters.MassTransitEventStorePublisher;

using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcedPM.Ports.EventStore;
using MassTransit;

public class MassTransitEventStorePublisherAdapter : IEventPublisher
{
    private readonly IBus bus;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MassTransitEventStorePublisherAdapter(IBus bus)
    {
        this.bus = bus;
    }

    public async Task Publish(IEnumerable<EventWithMetadata> events)
    {
        foreach (var evt in events)
        {
            await bus.Publish(evt.Event, evt.Event.GetType());
        }
    }
}
