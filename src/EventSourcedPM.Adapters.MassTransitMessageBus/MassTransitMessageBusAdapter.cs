namespace EventSourcedPM.Adapters.MassTransitMessageBus;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourcedPM.Ports.MessageBus;
using MassTransit;

public class MassTransitMessageBusAdapter(IBus bus) : IMessageBus
{
    private IBus Bus { get; } = bus;

    // In this demo, using Publish for commands to not worry about target queue name conventions

    public Task PublishEvent<T>(
        MessageWithMetadata<T> evt,
        CancellationToken cancellationToken = default
    ) =>
        Bus.Publish(
            evt.Message,
            evt.Message.GetType(),
            context =>
            {
                context.MessageId = evt.Metadata.MessageId;
                context.CorrelationId = evt.Metadata.CorrelationId;
                context.RequestId = evt.Metadata.CausationId;
            },
            cancellationToken
        );

    public Task PublishEvent(
        MessageWithMetadata evt,
        CancellationToken cancellationToken = default
    ) =>
        Bus.Publish(
            evt.Message,
            evt.Message.GetType(),
            context =>
            {
                context.MessageId = evt.Metadata.MessageId;
                context.CorrelationId = evt.Metadata.CorrelationId;
                context.RequestId = evt.Metadata.CausationId;
            },
            cancellationToken
        );

    public Task PublishEvent(
        object evt,
        Guid? correlationId = default,
        Guid? causationId = default,
        CancellationToken cancellationToken = default
    ) =>
        PublishEvent(
            new MessageWithMetadata(
                evt,
                new MessageMetadata(
                    evt.GetType().FullName,
                    Guid.NewGuid(),
                    correlationId ?? Guid.NewGuid(),
                    causationId
                )
            ),
            cancellationToken
        );

    public async Task PublishEvents<T>(
        IEnumerable<MessageWithMetadata<T>> events,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var evt in events)
        {
            await PublishEvent(evt, cancellationToken);
        }
    }

    public async Task PublishEvents(
        IEnumerable<MessageWithMetadata> events,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var evt in events ?? Enumerable.Empty<MessageWithMetadata>())
        {
            await PublishEvent(evt, cancellationToken);
        }
    }

    public Task PublishEvents(
        IEnumerable<object> events,
        Guid? correlationId = default,
        Guid? causationId = default,
        CancellationToken cancellationToken = default
    ) =>
        PublishEvents(
            events?.Select(evt => new MessageWithMetadata(
                evt,
                new MessageMetadata(
                    evt.GetType().FullName,
                    Guid.NewGuid(),
                    correlationId ?? Guid.NewGuid(),
                    causationId
                )
            )),
            cancellationToken
        );

    public Task SendCommand<T>(
        MessageWithMetadata<T> cmd,
        CancellationToken cancellationToken = default
    ) => PublishEvent(cmd, cancellationToken);

    public Task SendCommand(
        MessageWithMetadata cmd,
        CancellationToken cancellationToken = default
    ) => PublishEvent(cmd, cancellationToken);

    public Task SendCommand(
        object cmd,
        Guid? correlationId = default,
        Guid? causationId = default,
        CancellationToken cancellationToken = default
    ) => PublishEvent(cmd, correlationId, causationId, cancellationToken);

    public Task SendCommands(
        IEnumerable<object> commands,
        Guid? correlationId = default,
        Guid? causationId = default,
        CancellationToken cancellationToken = default
    ) => PublishEvents(commands, correlationId, causationId, cancellationToken);
}
