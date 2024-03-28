namespace EventSourcedPM.Ports.EventStore;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public readonly record struct EventStreamId
{
    private string Value { get; }

    private EventStreamId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        Value = value;
    }

    public static implicit operator string(EventStreamId id) => id.Value;

    public static implicit operator EventStreamId(string id) => new(id);

    public override string ToString() => Value;
}

public readonly record struct EventStreamVersion
{
    private long Value { get; }

    private EventStreamVersion(long value)
    {
        Value = value;
    }

    public static implicit operator long(EventStreamVersion id) => id.Value;

    public static implicit operator EventStreamVersion(long id) => new(id);

    public override string ToString() => Value.ToString();
}

public record EventMetadata(
    string EventTypeFullName,
    Guid EventId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime Timestamp
)
{
    public static EventMetadata NewEventMetadata(string eventTypeFullName, DateTime timestamp) =>
        new(eventTypeFullName, Guid.NewGuid(), Guid.NewGuid(), default, timestamp);
}

public record EventWithMetadata(object Event, EventMetadata Metadata);

public record EventStream(
    EventStreamId StreamId,
    EventStreamVersion StreamVersion,
    IReadOnlyCollection<EventWithMetadata> Events
);

public interface IEventTypeResolver
{
    string GetEventTypeName(object evt) => evt.GetType().Name;
    string GetEventTypeFullName(object evt) => evt.GetType().FullName;
    Type GetEventType(string eventTypeName);
}

public class EventTypeResolver<TEvent> : IEventTypeResolver
{
    public Type GetEventType(string eventTypeName) =>
        eventTypeByName.GetOrAdd(eventTypeName, x => typeof(TEvent).Assembly.GetType(x));

    private readonly ConcurrentDictionary<string, Type> eventTypeByName = new();
}

public interface IEventSerializer
{
    byte[] Serialize<T>(T instance);
    byte[] Serialize(object instance);

    T Deserialize<T>(ReadOnlySpan<byte> data);
    object Deserialize(ReadOnlySpan<byte> data, Type instanceType);
}

public class EventJsonSerializer : IEventSerializer
{
    public byte[] Serialize<T>(T instance) => JsonSerializer.SerializeToUtf8Bytes(instance);

    public byte[] Serialize(object instance) => JsonSerializer.SerializeToUtf8Bytes(instance);

    public T Deserialize<T>(ReadOnlySpan<byte> data) => JsonSerializer.Deserialize<T>(data);

    public object Deserialize(ReadOnlySpan<byte> data, Type instanceType) =>
        JsonSerializer.Deserialize(data, instanceType);
}

public interface IEventPublisher
{
    Task Publish(
        IEnumerable<EventWithMetadata> events,
        CancellationToken cancellationToken = default
    );
}

public class NoOpEventPublisher : IEventPublisher
{
    public Task Publish(
        IEnumerable<EventWithMetadata> events,
        CancellationToken cancellationToken = default
    ) => Task.CompletedTask;
}

public interface IEventStreamProjection<TState, in TEvent>
{
    TState GetInitialState(EventStreamId streamId);
    TState Apply(TState state, TEvent evt);
}

public interface IEventStreamSession<TState, out TEvent> : IDisposable, IAsyncDisposable
{
    Task<EventStream> GetAllEvents(
        TimeSpan deadline = default,
        CancellationToken cancellationToken = default
    );
    EventStream GetNewEvents();
    Task<TState> GetState(
        IEventStreamProjection<TState, TEvent> projection,
        TimeSpan deadline = default,
        CancellationToken cancellationToken = default
    );
    void AppendEvents(IEnumerable<object> events, Guid? correlationId, Guid? causationId);
    void AppendEvents(IEnumerable<EventWithMetadata> events);
    Task Save(TimeSpan deadline = default, CancellationToken cancellationToken = default);
}

public interface IEventStore<TState, out TEvent> : IDisposable, IAsyncDisposable
{
    IEventStreamSession<TState, TEvent> Open(EventStreamId streamId);
    Task<bool> Contains(EventStreamId streamId, CancellationToken cancellationToken = default);
}

public class UnknownEventTypeException(string typeName)
    : Exception($"Unknown event type {typeName}");

public class SessionIsLockedException(EventStreamId streamId)
    : Exception($"Session is locked for modifications for stream {streamId}");

public class ConcurrencyException(EventStreamId streamId, Exception innerException)
    : Exception($"Concurrency exception while saving stream {streamId}", innerException);
