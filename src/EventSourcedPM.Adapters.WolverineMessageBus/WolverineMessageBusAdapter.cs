using EventSourcedPM.Ports.MessageBus;

namespace EventSourcedPM.Adapters.WolverineMessageBus;

public class WolverineMessageBusAdapter(Wolverine.IMessageBus bus) : IMessageBus
{
    // In this demo, using Publish for commands to not worry about target queue name conventions

    public Task PublishEvent<T>(MessageWithMetadata<T> evt, CancellationToken cancellationToken = default) =>
        bus.PublishAsync(evt.Message, BuildDeliveryOptions(evt.Metadata)).AsTask();

    public Task PublishEvent(MessageWithMetadata evt, CancellationToken cancellationToken = default) =>
        bus.PublishAsync(evt.Message, BuildDeliveryOptions(evt.Metadata)).AsTask();

    public Task PublishEvent(object evt, Guid? correlationId = default, Guid? causationId = default, CancellationToken cancellationToken = default) =>
        PublishEvent(
            new MessageWithMetadata(evt, new MessageMetadata(evt.GetType().FullName, Guid.NewGuid(), correlationId ?? Guid.NewGuid(), causationId)),
            cancellationToken
        );

    public async Task PublishEvents<T>(IEnumerable<MessageWithMetadata<T>> events, CancellationToken cancellationToken = default)
    {
        foreach (var evt in events)
        {
            await PublishEvent(evt, cancellationToken);
        }
    }

    public async Task PublishEvents(IEnumerable<MessageWithMetadata> events, CancellationToken cancellationToken = default)
    {
        foreach (var evt in events ?? [])
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
                new MessageMetadata(evt.GetType().FullName, Guid.NewGuid(), correlationId ?? Guid.NewGuid(), causationId)
            )),
            cancellationToken
        );

    public Task SendCommand<T>(MessageWithMetadata<T> cmd, CancellationToken cancellationToken = default) => PublishEvent(cmd, cancellationToken);

    public Task SendCommand(MessageWithMetadata cmd, CancellationToken cancellationToken = default) => PublishEvent(cmd, cancellationToken);

    public Task SendCommand(object cmd, Guid? correlationId = default, Guid? causationId = default, CancellationToken cancellationToken = default) =>
        PublishEvent(cmd, correlationId, causationId, cancellationToken);

    public Task SendCommands(
        IEnumerable<object> commands,
        Guid? correlationId = default,
        Guid? causationId = default,
        CancellationToken cancellationToken = default
    ) => PublishEvents(commands, correlationId, causationId, cancellationToken);

    private static Wolverine.DeliveryOptions BuildDeliveryOptions(MessageMetadata metadata)
    {
        var options = new Wolverine.DeliveryOptions();
        options.Headers["correlation-id"] = metadata.CorrelationId.ToString();
        options.Headers["message-id"] = metadata.MessageId.ToString();
        if (metadata.CausationId.HasValue)
            options.Headers["causation-id"] = metadata.CausationId.Value.ToString();
        return options;
    }
}
