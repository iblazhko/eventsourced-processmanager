namespace EventSourcedPM.Adapters.MartenDbEventStore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcedPM.Ports.EventStore;
using Marten;
using Serilog;
using EventStreamId = System.String;
using EventStreamVersion = System.Int64;

internal sealed class MartenDbEventStreamSession<TState, TEvent>
    : IEventStreamSession<TState, TEvent>
{
    private readonly IEventPublisher eventPublisher;
    private readonly IDocumentSession session;
    private readonly EventStreamId streamId;
    private readonly List<EventWithMetadata> savedEvents;
    private readonly List<EventWithMetadata> newEvents;
    private EventStreamVersion savedVersion;

    private IEnumerable<EventWithMetadata> ConcatenatedEvents => savedEvents.Concat(newEvents);
    private EventStreamVersion NewVersion => savedVersion + newEvents.Count;

    private bool isLocked;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MartenDbEventStreamSession(
        IDocumentStore documentStore,
        IEventPublisher eventPublisher,
        EventStreamId streamId
    )
    {
        this.eventPublisher = eventPublisher;

        session = documentStore.LightweightSession();
        this.streamId = streamId;
        savedEvents = new();
        newEvents = new();
        savedVersion = 0;
        isLocked = false;
    }

    private IEnumerable<EventWithMetadata> MapFromMartenEvents(
        IReadOnlyList<Marten.Events.IEvent> mtEvents
    ) => mtEvents.Select(e => new EventWithMetadata(e.Data, e.EventType.FullName, null));

    private void AssertSessionIsNotLocked()
    {
        if (isLocked)
            throw new SessionIsLockedException(streamId);
    }

    public async Task Open()
    {
        var mtEvents = await session.Events.FetchStreamAsync(streamId);

        if (mtEvents?.Count > 0)
        {
            savedEvents.AddRange(MapFromMartenEvents(mtEvents));
            savedVersion = mtEvents[^1].Version;
        }
    }

    public Task<EventStream> GetAllEvents() =>
        Task.FromResult(new EventStream(streamId, NewVersion, ConcatenatedEvents.ToList()));

    public Task<EventStream> GetNewEvents() =>
        Task.FromResult(new EventStream(streamId, NewVersion, newEvents));

    public Task<TState> GetState(IEventStreamProjection<TState, TEvent> projection)
    {
        var seed = projection.GetInitialState(streamId);
        var state = ConcatenatedEvents.Aggregate(
            seed,
            (s, e) => projection.Apply(s, (TEvent)e.Event)
        );

        return Task.FromResult(state);
    }

    public Task AppendEvents(IEnumerable<object> events)
    {
        AssertSessionIsNotLocked();
        newEvents.AddRange(
            events
                ?.Where(e => e != null)
                .Select(e => new EventWithMetadata(e, e.GetType().FullName, null))
                ?? Enumerable.Empty<EventWithMetadata>()
        );

        return Task.CompletedTask;
    }

    public Task AppendEvents(IEnumerable<EventWithMetadata> events)
    {
        AssertSessionIsNotLocked();
        newEvents.AddRange(
            events?.Where(e => e is { Event: not null }) ?? Enumerable.Empty<EventWithMetadata>()
        );

        return Task.CompletedTask;
    }

    public async Task Save()
    {
        Log.Debug("[EVENTSTORE] Save event stream {EventStreamId}", streamId);
        if (newEvents.Count == 0)
            return;

        var mtEvents = newEvents.Select(e => e.Event);
        _ = savedVersion switch
        {
            0 => session.Events.StartStream(streamId, mtEvents),
            _ => session.Events.Append(streamId, mtEvents)
        };

        await session.SaveChangesAsync();
        Lock();

        await eventPublisher.Publish(newEvents);
    }

    public void Lock()
    {
        isLocked = true;
    }

    public void Dispose()
    {
        session?.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return session?.DisposeAsync() ?? ValueTask.CompletedTask;
    }
}

public sealed class MartenDbEventStoreAdapter<TState, TEvent> : IEventStore<TState, TEvent>
{
    private readonly IDocumentStore documentStore;
    private readonly IEventPublisher eventPublisher;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MartenDbEventStoreAdapter(IDocumentStore documentStore, IEventPublisher eventPublisher)
    {
        this.documentStore = documentStore;
        this.eventPublisher = eventPublisher;
    }

    public async Task<IEventStreamSession<TState, TEvent>> Open(string streamId)
    {
        Log.Debug("[EVENTSTORE] Open event stream {EventStreamId}", streamId);
        var mtSession = new MartenDbEventStreamSession<TState, TEvent>(
            documentStore,
            eventPublisher,
            streamId
        );
        await mtSession.Open();

        return mtSession;
    }

    public Task Delete(EventStreamId streamId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Contains(EventStreamId streamId)
    {
        throw new NotImplementedException();
    }

    // MartenDb IDocumentStore instance lifecycle is managed by the application host
    // hence no disposing is necessary in this adapter
    public void Dispose() { }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
