namespace EventSourcedPM.Adapters.MartenDbEventStore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EventSourcedPM.Ports.EventStore;
using Marten;
using Serilog;

internal sealed class MartenDbEventStreamSession<TState, TEvent>(
    EventStreamId streamId,
    IDocumentStore documentStore,
    IEventPublisher eventPublisher,
    TimeProvider timeProvider
) : IEventStreamSession<TState, TEvent>
{
    private IEventPublisher EventPublisher { get; } = eventPublisher;
    private IDocumentSession Session { get; } = documentStore.LightweightSession();
    private TimeProvider EventTimeProvider { get; } = timeProvider;
    private EventStreamId StreamId { get; } = streamId;
    private readonly List<EventWithMetadata> storedEvents = new();
    private readonly List<EventWithMetadata> newEvents = new();
    private EventStreamVersion storedRevision = 0;

    private IEnumerable<EventWithMetadata> AllEvents => storedEvents.Concat(newEvents);
    private EventStreamVersion Revision => storedRevision + newEvents.Count;

    private bool isLocked;

    private IEnumerable<EventWithMetadata> MapFromMartenEvents(
        IReadOnlyList<Marten.Events.IEvent> mtEvents
    ) =>
        mtEvents.Select(e => new EventWithMetadata(
            e.Data,
            EventMetadata.NewEventMetadata(
                e.EventType.FullName,
                EventTimeProvider.GetUtcNow().UtcDateTime
            )
        ));

    private void AssertSessionIsNotLocked()
    {
        if (isLocked)
            throw new SessionIsLockedException(StreamId);
    }

    private async Task ReadStoredEvents(CancellationToken cancellationToken)
    {
        Log.Debug("[EVENTSTORE] Open event stream {EventStreamId}", StreamId);

        var mtEvents = await Session.Events.FetchStreamAsync(StreamId, token: cancellationToken);

        // ReSharper disable once ConstantConditionalAccessQualifier
        if (mtEvents?.Count > 0)
        {
            storedEvents.AddRange(MapFromMartenEvents(mtEvents));

            var incompatibleEvents = storedEvents
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

            storedRevision = mtEvents[^1].Version;
        }
    }

    public async Task<EventStream> GetAllEvents(
        TimeSpan deadline = default,
        CancellationToken cancellationToken = default
    )
    {
        if (storedEvents.Count == 0)
            await ReadStoredEvents(cancellationToken);
        return new EventStream(StreamId, Revision, AllEvents.ToList());
    }

    public EventStream GetNewEvents() => new(StreamId, Revision, newEvents);

    public async Task<TState> GetState(
        IEventStreamProjection<TState, TEvent> projection,
        TimeSpan deadline = default,
        CancellationToken cancellationToken = default
    )
    {
        if (storedEvents.Count == 0)
            await ReadStoredEvents(cancellationToken);

        var seed = projection.GetInitialState(StreamId);
        var state = AllEvents.Aggregate(seed, (s, e) => projection.Apply(s, (TEvent)e.Event));

        return state;
    }

    public void AppendEvents(
        IEnumerable<object> events,
        Guid? correlationId = default,
        Guid? causationId = default
    ) =>
        AppendEvents(
            events?.Select(e => new EventWithMetadata(
                e,
                new EventMetadata(
                    e.GetType().FullName,
                    Guid.NewGuid(),
                    correlationId ?? Guid.NewGuid(),
                    causationId,
                    EventTimeProvider.GetUtcNow().UtcDateTime
                )
            ))
        );

    public void AppendEvents(IEnumerable<EventWithMetadata> events)
    {
        AssertSessionIsNotLocked();
        newEvents.AddRange(
            events
                ?.Where(e => e is { Event: not null })
                .Select(e =>
                    EventTypeIsCompatible(e.Event)
                        ? e
                        : throw new InvalidOperationException(
                            $"Event ${e.Event.GetType().FullName} is not compatible with ${typeof(TEvent).FullName}"
                        )
                ) ?? []
        );
    }

    public async Task Save(
        TimeSpan deadline = default,
        CancellationToken cancellationToken = default
    )
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
        _ = (long)storedRevision switch
        {
            0 => Session.Events.StartStream(StreamId, mtEvents),
            _ => Session.Events.Append(StreamId, mtEvents)
        };

        try
        {
            await Session.SaveChangesAsync(cancellationToken);
            Lock();

            await EventPublisher.Publish(newEvents, cancellationToken);
        }
        catch (Marten.Exceptions.EventStreamUnexpectedMaxEventIdException e)
        {
            throw new ConcurrencyException(StreamId, e);
        }
    }

    private void Lock()
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
