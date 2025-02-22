namespace EventSourcedPM.Ports.MessageBus;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public record MessageMetadata(string MessageTypeFullName, Guid MessageId, Guid CorrelationId, Guid? CausationId);

public record MessageWithMetadata(object Message, MessageMetadata Metadata);

public record MessageWithMetadata<T>(T Message, MessageMetadata Metadata);

public interface IMessageBus
{
    Task PublishEvent<T>(MessageWithMetadata<T> evt, CancellationToken cancellationToken = default);
    Task PublishEvent(MessageWithMetadata evt, CancellationToken cancellationToken = default);
    Task PublishEvent(object evt, Guid? correlationId = default, Guid? causationId = default, CancellationToken cancellationToken = default);

    Task PublishEvents<T>(IEnumerable<MessageWithMetadata<T>> events, CancellationToken cancellationToken = default);
    Task PublishEvents(IEnumerable<MessageWithMetadata> events, CancellationToken cancellationToken = default);
    Task PublishEvents(
        IEnumerable<object> events,
        Guid? correlationId = default,
        Guid? causationId = default,
        CancellationToken cancellationToken = default
    );

    Task SendCommand<T>(MessageWithMetadata<T> cmd, CancellationToken cancellationToken = default);
    Task SendCommand(MessageWithMetadata cmd, CancellationToken cancellationToken = default);
    Task SendCommand(object cmd, Guid? correlationId = default, Guid? causationId = default, CancellationToken cancellationToken = default);
    Task SendCommands(
        IEnumerable<object> commands,
        Guid? correlationId = default,
        Guid? causationId = default,
        CancellationToken cancellationToken = default
    );
}
