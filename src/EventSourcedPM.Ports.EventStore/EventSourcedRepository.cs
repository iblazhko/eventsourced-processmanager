namespace EventSourcedPM.Ports.EventStore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStreamId = System.String;

public class EventSourcedRepository<TState, TEvent>
{
    private readonly IEventStore<TState, TEvent> eventStore;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EventSourcedRepository(IEventStore<TState, TEvent> eventStore)
    {
        this.eventStore = eventStore;
    }

    public async Task<TState> Read(
        EventStreamId streamId,
        IEventStreamProjection<TState, TEvent> stateProjection
    )
    {
        var session = await eventStore.Open(streamId);
        var state = await session.GetState(stateProjection);

        return state;
    }

    public async Task<IEnumerable<TEvent>> Upsert(
        EventStreamId streamId,
        IEventStreamProjection<TState, TEvent> stateProjection,
        Func<TState, IEnumerable<TEvent>> action
    )
    {
        var session = await eventStore.Open(streamId);
        var state = await session.GetState(stateProjection);
        var newEvents = (action(state) ?? Enumerable.Empty<TEvent>()).ToList();
        if (newEvents.Count > 0)
        {
            await session.AppendEvents(newEvents.Cast<object>().AsEnumerable());
            await session.Save();
        }

        return newEvents;
    }
}
