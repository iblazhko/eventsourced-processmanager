namespace EventSourcedPM.Adapters.MartenDbEventStore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EventSourcedPM.Ports.EventStore;
using Marten;
using Serilog;
using EventStreamId = string;
using EventStreamVersion = long;

internal sealed class MartenDbEventStreamSession<TState, TEvent>(
    IDocumentStore documentStore,
    IEventPublisher eventPublisher,
    EventStreamId streamId
) : IEventStreamSession<TState, TEvent>
{
    private IEventPublisher EventPublisher { get; } = eventPublisher;
    private IDocumentSession Session { get; } = documentStore.LightweightSession();
    private EventStreamId StreamId { get; } = streamId;
    private readonly List<EventWithMetadata> savedEvents = new();
    private readonly List<EventWithMetadata> newEvents = new();
    private EventStreamVersion savedVersion = 0;

    private IEnumerable<EventWithMetadata> AllEvents => savedEvents.Concat(newEvents);
    private EventStreamVersion NewVersion => savedVersion + newEvents.Count;

    private bool isLocked = false;

    private IEnumerable<EventWithMetadata> MapFromMartenEvents(
        IReadOnlyList<Marten.Events.IEvent> mtEvents
    ) => mtEvents.Select(e => new EventWithMetadata(e.Data, e.EventType.FullName, null));

    private void AssertSessionIsNotLocked()
    {
        if (isLocked)
            throw new SessionIsLockedException(StreamId);
    }

    public async Task Open()
    {
        var mtEvents = await Session.Events.FetchStreamAsync(StreamId);

        if (mtEvents?.Count > 0)
        {
            savedEvents.AddRange(MapFromMartenEvents(mtEvents));

            var incompatibleEvents = savedEvents
                .Where(e => !EventTypeIsCompatible(e.Event))
                .Select(e => e.GetType().FullName)
                .Distinct()
                .ToList();
            if (incompatibleEvents.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Event stream {StreamId} contains events not compatible with {typeof(TEvent).FullName}: {string.Join(", ", incompatibleEvents)}"
                );
            }

            savedVersion = mtEvents[^1].Version;
        }
    }

    public Task<EventStream> GetAllEvents() =>
        Task.FromResult(new EventStream(StreamId, NewVersion, AllEvents.ToList()));

    public Task<EventStream> GetNewEvents() =>
        Task.FromResult(new EventStream(StreamId, NewVersion, newEvents));

    public Task<TState> GetState(IEventStreamProjection<TState, TEvent> projection)
    {
        var seed = projection.GetInitialState(StreamId);
        var state = AllEvents.Aggregate(seed, (s, e) => projection.Apply(s, (TEvent)e.Event));

        return Task.FromResult(state);
    }

    public Task AppendEvents(IEnumerable<object> events)
    {
        AssertSessionIsNotLocked();
        newEvents.AddRange(
            events
                ?.Where(e => e != null)
                .Select(e =>
                    EventTypeIsCompatible(e)
                        ? new EventWithMetadata(e, e.GetType().FullName, null)
                        : throw new InvalidOperationException(
                            $"Event ${e.GetType().FullName} is not compatible with ${typeof(TEvent).FullName}"
                        )
                ) ?? Enumerable.Empty<EventWithMetadata>()
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
        Log.Debug("[EVENTSTORE] Save event stream {EventStreamId}", StreamId);
        if (newEvents.Count == 0)
            return;

        var incompatibleEvents = newEvents
            .Where(e => !EventTypeIsCompatible(e.Event))
            .Select(e => e.GetType().FullName)
            .Distinct()
            .ToList();
        if (incompatibleEvents.Count > 0)
        {
            throw new InvalidOperationException(
                $"Event stream {StreamId} session contains events not compatible with {typeof(TEvent).FullName}: {string.Join(", ", incompatibleEvents)}"
            );
        }

        var mtEvents = newEvents.Select(e => e.Event);
        _ = savedVersion switch
        {
            0 => Session.Events.StartStream(StreamId, mtEvents),
            _ => Session.Events.Append(StreamId, mtEvents)
        };

        await Session.SaveChangesAsync();
        Lock();

        await EventPublisher.Publish(newEvents);
    }

    public void Lock()
    {
        isLocked = true;
    }

    public void Dispose()
    {
        Session?.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return Session?.DisposeAsync() ?? ValueTask.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool EventTypeIsCompatible(object evt) => evt.GetType().IsAssignableTo(typeof(TEvent));
}

public sealed class MartenDbEventStoreAdapter<TState, TEvent>(
    IDocumentStore documentStore,
    IEventPublisher eventPublisher
) : IEventStore<TState, TEvent>
{
    private IDocumentStore DocumentStore { get; } = documentStore;
    private IEventPublisher EventPublisher { get; } = eventPublisher;

    public async Task<IEventStreamSession<TState, TEvent>> Open(string streamId)
    {
        Log.Debug("[EVENTSTORE] Open event stream {EventStreamId}", streamId);
        var mtSession = new MartenDbEventStreamSession<TState, TEvent>(
            DocumentStore,
            EventPublisher,
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
