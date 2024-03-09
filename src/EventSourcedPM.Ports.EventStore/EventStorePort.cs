using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcedPM.Ports.EventStore;

using EventId = System.Guid;
using EventStreamId = System.String;
using EventStreamVersion = System.Int64;

// TODO: Populate events metadata
public record EventMetadata(EventId EventId, EventId CorrelationId, EventId CausationId);

public record EventWithMetadata(object Event, string EventTypeName, EventMetadata Metadata);

public record EventStream(
    EventStreamId StreamId,
    EventStreamVersion StreamVersion,
    ICollection<EventWithMetadata> Events
);

public interface IEventPublisher
{
    Task Publish(IEnumerable<EventWithMetadata> events);
}

public interface IEventStreamProjection<TState, TEvent>
{
    TState GetInitialState(EventStreamId streamId);
    TState Apply(TState state, TEvent evt);
}

public interface IEventStreamSession<TState, TEvent> : IDisposable, IAsyncDisposable
{
    Task<EventStream> GetAllEvents();
    Task<EventStream> GetNewEvents();
    Task<TState> GetState(IEventStreamProjection<TState, TEvent> projection);
    Task AppendEvents(IEnumerable<object> events);
    Task AppendEvents(IEnumerable<EventWithMetadata> events);
    Task Save();

    void Lock();
}

public interface IEventStore<TState, TEvent> : IDisposable, IAsyncDisposable
{
    Task<IEventStreamSession<TState, TEvent>> Open(EventStreamId streamId);
    Task Delete(EventStreamId streamId);
    Task<bool> Contains(EventStreamId streamId);
}

public class UnknownEventTypeException(string typeName)
    : Exception($"Unknown event type {typeName}");

public class SessionIsLockedException(EventStreamId streamId)
    : Exception($"Session is locked for modifications for stream {streamId}");

public class ConcurrencyException(EventStreamId streamId, Exception innerException)
    : Exception($"Concurrency exception while saving stream {streamId}", innerException);
