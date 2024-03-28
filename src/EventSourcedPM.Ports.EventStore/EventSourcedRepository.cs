namespace EventSourcedPM.Ports.EventStore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class EventSourcedRepository<TState, TEvent>(IEventStore<TState, TEvent> eventStore)
{
    private IEventStore<TState, TEvent> EventStore { get; } = eventStore;

    public async Task<TState> GetState(
        EventStreamId streamId,
        IEventStreamProjection<TState, TEvent> stateProjection,
        TimeSpan deadline = default,
        CancellationToken cancellationToken = default
    )
    {
        await using var session = EventStore.Open(streamId);
        var state = await session.GetState(stateProjection, deadline, cancellationToken);

        return state;
    }

    public async Task<IEnumerable<TEvent>> AddEvents(
        EventStreamId streamId,
        IEventStreamProjection<TState, TEvent> stateProjection,
        Func<TState, IEnumerable<TEvent>> action,
        Guid? correlationId = default,
        Guid? causationId = default,
        TimeSpan deadline = default,
        CancellationToken cancellationToken = default
    )
    {
        await using var session = EventStore.Open(streamId);
        var state = await session.GetState(stateProjection, deadline, cancellationToken);
        var newEvents = (action(state) ?? []).ToList();
        if (newEvents.Count > 0)
        {
            session.AppendEvents(
                newEvents.Cast<object>().AsEnumerable(),
                correlationId,
                causationId
            );
            await session.Save(deadline, cancellationToken);
        }

        return newEvents;
    }
}
