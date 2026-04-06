using System.Text;

namespace EventSourcedPM.Configuration;

public class ShipmentProcessSettings
{
    public string ApiBaseUrl { get; init; }
    public string EventStoreAdapter { get; init; } = "KurrentDB";
    public string MessageBusAdapter { get; init; } = "MassTransit";
    public EventStoreSettings EventStore { get; init; }
    public PostgresSettings Postgres { get; init; }
    public RabbitMqSettings RabbitMq { get; init; }
    public MassTransitSettings MassTransit { get; init; }
    public WolverineSettings Wolverine { get; init; }
    public LoggingSettings Logging { get; init; }
    public bool WaitForInfrastructureOnStartup { get; init; }

    public override string ToString() =>
        new StringBuilder()
            .AppendSettingTitle("Shipment Process")
            .AppendSettingValue(() => ApiBaseUrl)
            .AppendLine()
            .AppendSubSection(() => EventStoreAdapter)
            .AppendSubSection(() => MessageBusAdapter)
            .AppendSubSection(() => EventStore)
            .AppendSubSection(() => Postgres)
            .AppendSubSection(() => RabbitMq)
            .AppendSubSection(() => MassTransit)
            .AppendSubSection(() => Wolverine)
            .AppendSubSection(() => Logging)
            .AppendLine()
            .AppendSettingValue(() => WaitForInfrastructureOnStartup)
            .ToString();
}
