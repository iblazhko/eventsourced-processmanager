namespace EventSourcedPM.Ports.EventStore;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public readonly record struct EventId
{
    private Guid Value { get; }

    private EventId(Guid value)
    {
        Value = value;
    }

    public static EventId NewId() => new(Guid.NewGuid());

    public static EventId Empty => new(Guid.Empty);

    public static implicit operator Guid(EventId id) => id.Value;

    public static implicit operator EventId(Guid id) => new(id);

    public override string ToString() => Value.ToString();
}

public readonly record struct EventStreamId
{
    private string Value { get; }

    private EventStreamId(string value)
    {
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

public record EventMetadata(EventId EventId, EventId CorrelationId, EventId? CausationId)
{
    public static EventMetadata NewEventMetadata() =>
        new(EventId.NewId(), EventId.NewId(), default);
}

public record EventWithMetadata(object Event, string EventTypeFullName, EventMetadata Metadata);

public record EventStream(
    EventStreamId StreamId,
    EventStreamVersion StreamVersion,
    ICollection<EventWithMetadata> Events
);

public interface IEventTypeResolver
{
    string GetEventTypeName(object evt) => evt.GetType().Name;
    string GetEventTypeFullName<T>(object evt) => evt.GetType().FullName;
    Type GetEventType(string eventTypeName);
}

public class EventTypeResolver<TEvent> : IEventTypeResolver
{
    public Type GetEventType(string eventTypeName) => GetEventTypeFromName(eventTypeName);

    private readonly Dictionary<string, Type> eventTypeByName = new();

    private Type GetEventTypeFromName(string eventTypeName)
    {
        Type eventType;
        if (!eventTypeByName.ContainsKey(eventTypeName))
        {
            var t = typeof(TEvent).Assembly.GetType(eventTypeName);

            eventTypeByName.Add(eventTypeName, t);
            eventType = t;
        }
        else
        {
            eventType = eventTypeByName[eventTypeName];
        }

        return eventType;
    }
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
    public static IEventSerializer Instance { get; } = new EventJsonSerializer();

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
    public static IEventPublisher Instance { get; } = new NoOpEventPublisher();

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
    void AppendEvents(IEnumerable<object> events);
    void AppendEvents(IEnumerable<EventWithMetadata> events);
    Task Save(TimeSpan deadline = default, CancellationToken cancellationToken = default);

    void Lock();
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
