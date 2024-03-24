namespace EventSourcedPM.Adapters.EventStoreDb;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EventSourcedPM.Ports.EventStore;
using EventStore.Client;
using LanguageExt;
using Serilog;
using static LanguageExt.Prelude;

internal sealed class EventStoreDbEventStreamSession<TState, TEvent>(
    EventStreamId streamId,
    EventStoreClient client,
    IEventPublisher eventPublisher,
    IEventTypeResolver eventTypeResolver,
    IEventSerializer eventSerializer,
    TimeProvider timeProvider
) : IEventStreamSession<TState, TEvent>
{
    private EventStoreClient Client { get; } = client;
    private IEventPublisher EventPublisher { get; } = eventPublisher;
    private IEventTypeResolver EventTypeResolver { get; } = eventTypeResolver;
    private IEventSerializer EventSerializer { get; } = eventSerializer;
    private TimeProvider EventTimeProvider { get; } = timeProvider;

    private EventStreamId StreamId { get; } = streamId;

    // Only used when calling code explicitly asks for all events (GetAllEvents)
    private readonly List<EventWithMetadata> storedEvents = new();

    private readonly List<EventWithMetadata> newEvents = new();
    private Option<EventStreamVersion> storedRevision = None;
    private bool knownStoredRevision;
    private Option<TState> stateFromStoredEvents = None;

    // If we have opened event stream (typically via GetState), we know the last event's revision.
    // Otherwise (typically when a calling code uses AppendEvents without using GetState), we need to read last event before Save

    private EventStreamVersion Revision => storedRevision.Some(r => r).None(0) + newEvents.Count;

    private bool isLocked;

    public async Task<EventStream> GetAllEvents(
        TimeSpan deadline = default,
        CancellationToken cancellationToken = default
    )
    {
        if (storedEvents.Count == 0)
            await ReadStoredEvents(DeadlineOrDefault(deadline), cancellationToken);

        return new EventStream(StreamId, Revision, storedEvents.Concat(newEvents).ToList());
    }

    public EventStream GetNewEvents() => new(StreamId, Revision, newEvents);

    private async Task<TState> GetStateFromStoredEvents(
        IEventStreamProjection<TState, TEvent> projection,
        TimeSpan deadline = default,
        CancellationToken cancellationToken = default
    )
    {
        var currentState = projection.GetInitialState(StreamId);

        await ProcessStoredEvents(
            x =>
            {
                currentState = projection.Apply(currentState, (TEvent)x.Event);
            },
            DeadlineOrDefault(deadline),
            cancellationToken
        );

        stateFromStoredEvents = Some(currentState);

        return currentState;
    }

    public async Task<TState> GetState(
        IEventStreamProjection<TState, TEvent> projection,
        TimeSpan deadline = default,
        CancellationToken cancellationToken = default
    )
    {
        var currentState = stateFromStoredEvents
            .Some(s => s)
            .None(
                await GetStateFromStoredEvents(
                    projection,
                    DeadlineOrDefault(deadline),
                    cancellationToken
                )
            );

        return newEvents.Aggregate(currentState, (s, e) => projection.Apply(s, (TEvent)e.Event));
    }

    public void AppendEvents(
        IEnumerable<object> events,
        Guid? correlationId = default,
        Guid? causationId = default
    ) =>
        AppendEvents(
            events
                ?.Where(e => e != null)
                .Select(e => new EventWithMetadata(
                    e,
                    new EventMetadata(
                        EventTypeResolver.GetEventTypeFullName(e),
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
                ) ?? Enumerable.Empty<EventWithMetadata>()
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

        // TODO: This could be redundant - already checking compatibility in AppendEvents (at least in one overload)
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

        if (!knownStoredRevision)
            await GetLastRevision(DeadlineOrDefault(deadline), cancellationToken);

        var writeResult = await Client.AppendToStreamAsync(
            StreamId,
            storedRevision.Some(r => StreamRevision.FromInt64(r)).None(StreamRevision.None),
            newEvents.Select(evt => new EventData(
                Uuid.NewUuid(),
                EventTypeResolver.GetEventTypeName(evt.Event),
                EventSerializer.Serialize(evt.Event),
                EventSerializer.Serialize(evt.Metadata)
            )),
            deadline: DeadlineOrDefault(deadline),
            cancellationToken: cancellationToken
        );

        if (writeResult is WrongExpectedVersionResult)
            throw new ConcurrencyException(StreamId, null);

        Lock();

        await EventPublisher.Publish(newEvents, cancellationToken);
    }

    private void Lock()
    {
        isLocked = true;
    }

    #region Dispose

    // EventStoreClient manages its internal state, no disposal is needed in this adapter

    public void Dispose() { }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    #endregion

    private async Task GetLastRevision(TimeSpan deadline, CancellationToken cancellationToken)
    {
        Log.Debug(
            "[EVENTSTORE] Open event stream {EventStreamId} (getting last event revision)",
            (string)StreamId
        );
        var readResult = Client.ReadStreamAsync(
            Direction.Backwards,
            StreamId,
            StreamPosition.End,
            resolveLinkTos: false,
            deadline: deadline,
            cancellationToken: cancellationToken
        );

        if (await StreamExists(readResult))
        {
            storedRevision = Optional(readResult.LastStreamPosition?.ToInt64())
                .Some(r => Some((EventStreamVersion)r))
                .None(None);
        }

        knownStoredRevision = true;
    }

    private async Task ProcessStoredEvents(
        Action<EventWithMetadata> action,
        TimeSpan deadline,
        CancellationToken cancellationToken
    )
    {
        Log.Debug(
            "[EVENTSTORE] Open event stream {EventStreamId} (getting stored events)",
            (string)StreamId
        );

        var readResult = Client.ReadStreamAsync(
            Direction.Forwards,
            StreamId,
            StreamPosition.Start,
            resolveLinkTos: true,
            deadline: deadline,
            cancellationToken: cancellationToken
        );

        if (await StreamExists(readResult))
        {
            await foreach (var e in readResult)
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (e.Event == null)
                    continue;

                var evtMetadata = e.Event.Metadata.Span.IsEmpty
                    ? EventMetadata.NewEventMetadata(
                        e.Event.EventType,
                        EventTimeProvider.GetUtcNow().UtcDateTime
                    )
                    : EventSerializer.Deserialize<EventMetadata>(e.Event.Metadata.Span);

                var eventType = EventTypeResolver.GetEventType(evtMetadata.EventTypeFullName);
                if (eventType == default || !EventTypeIsCompatible(eventType))
                {
                    throw new InvalidOperationException(
                        $"Event stream {StreamId} contains events not compatible with {typeof(TEvent).FullName}: {evtMetadata.EventTypeFullName}"
                    );
                }

                var evt = EventSerializer.Deserialize(e.Event.Data.Span, eventType);
                action(new EventWithMetadata(evt, evtMetadata));
                storedRevision = Some((EventStreamVersion)e.Event.EventNumber.ToInt64());
            }
        }

        knownStoredRevision = true;
    }

    private async Task ReadStoredEvents(TimeSpan deadline, CancellationToken cancellationToken)
    {
        await ProcessStoredEvents(
            x =>
            {
                storedEvents.Add(x);
            },
            deadline,
            cancellationToken
        );
    }

    private void AssertSessionIsNotLocked()
    {
        if (isLocked)
            throw new SessionIsLockedException(StreamId);
    }

    private async Task<bool> StreamExists(EventStoreClient.ReadStreamResult readResult)
    {
        try
        {
            var readState = await readResult.ReadState;
            return readState == ReadState.Ok;
        }
        catch (StreamDeletedException)
        {
            return false;
        }
    }

    // ReSharper disable once StaticMemberInGenericType
    private static readonly TimeSpan DefaultDeadline = TimeSpan.FromSeconds(30);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TimeSpan DeadlineOrDefault(TimeSpan deadline) =>
        deadline == default ? DefaultDeadline : deadline;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool EventTypeIsCompatible(object evt) => EventTypeIsCompatible(evt.GetType());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool EventTypeIsCompatible(Type eventType) => eventType.IsAssignableTo(typeof(TEvent));
}
