namespace EventSourcedPM.Adapters.EventStoreDb;

using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourcedPM.Ports.EventStore;
using EventStore.Client;

public class EventStoreDbAdapter<TState, TEvent>(
    EventStoreClient client,
    IEventPublisher eventPublisher,
    IEventTypeResolver eventTypeResolver,
    IEventSerializer eventSerializer,
    TimeProvider timeProvider
) : IEventStore<TState, TEvent>
{
    private EventStoreClient Client { get; } = client;
    private IEventPublisher EventPublisher { get; } = eventPublisher;
    private IEventTypeResolver EventTypeResolver { get; } = eventTypeResolver;
    private IEventSerializer EventSerializer { get; } = eventSerializer;
    private TimeProvider EventTimeProvider { get; } = timeProvider;

    public IEventStreamSession<TState, TEvent> Open(EventStreamId streamId) =>
        new EventStoreDbEventStreamSession<TState, TEvent>(streamId, Client, EventPublisher, EventTypeResolver, EventSerializer, EventTimeProvider);

    public async Task<bool> Contains(EventStreamId streamId, CancellationToken cancellationToken = default)
    {
        try
        {
            var readResult = Client.ReadStreamAsync(
                Direction.Forwards,
                streamId,
                StreamPosition.Start,
                resolveLinkTos: false,
                cancellationToken: cancellationToken
            );

            var readState = await readResult.ReadState;
            return readState == ReadState.Ok;
        }
        catch (StreamDeletedException)
        {
            return false;
        }
    }

    // EventStoreClient instance lifecycle is managed by the application host
    // hence no disposing is necessary in this adapter

    public void Dispose() { }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
