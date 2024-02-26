namespace EventSourcedPM.Adapters.MassTransitMessageBus;

using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcedPM.Ports.MessageBus;
using MassTransit;

public class MassTransitMessageBusAdapter(IBus bus) : IMessageBus
{
    private IBus Bus { get; } = bus;

    public Task PublishEvent(object evt) => Bus.Publish(evt, evt.GetType());

    public async Task PublishEvents(IEnumerable<object> events)
    {
        foreach (var evt in events)
        {
            await PublishEvent(evt);
        }
    }

    // In this demo, using Publish for commands to not worry about target queue name conventions

    public Task SendCommand(object cmd) => PublishEvent(cmd);

    public Task SendCommands(IEnumerable<object> commands) => PublishEvents(commands);
}
