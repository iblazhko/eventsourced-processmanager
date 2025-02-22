namespace EventSourcedPM.Adapters.MartenDbEventStore;

using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourcedPM.Ports.EventStore;
using Marten;

public sealed class MartenDbEventStoreAdapter<TState, TEvent>(IDocumentStore documentStore, IEventPublisher eventPublisher, TimeProvider timeProvider)
    : IEventStore<TState, TEvent>
{
    private IDocumentStore DocumentStore { get; } = documentStore;
    private IEventPublisher EventPublisher { get; } = eventPublisher;
    private TimeProvider EventTimeProvider { get; } = timeProvider;

    public IEventStreamSession<TState, TEvent> Open(EventStreamId streamId) =>
        new MartenDbEventStreamSession<TState, TEvent>(streamId, DocumentStore, EventPublisher, EventTimeProvider);

    public Task Delete(EventStreamId streamId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Contains(EventStreamId streamId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    // MartenDb IDocumentStore instance lifecycle is managed by the application host
    // hence no disposing is necessary in this adapter
    public void Dispose() { }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
