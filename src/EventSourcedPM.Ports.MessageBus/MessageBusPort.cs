namespace EventSourcedPM.Ports.MessageBus;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IMessageBus
{
    Task PublishEvent(object evt);
    Task PublishEvents(IEnumerable<object> events);
    Task SendCommand(object cmd);
    Task SendCommands(IEnumerable<object> commands);
}
