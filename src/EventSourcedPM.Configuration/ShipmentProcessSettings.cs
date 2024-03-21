namespace EventSourcedPM.Configuration;

using System.Text;

public class ShipmentProcessSettings
{
    public string ApiBaseUrl { get; init; }
    public EventStoreSettings EventStore { get; init; }
    public PostgresSettings Postgres { get; init; }
    public RabbitMqSettings RabbitMq { get; init; }
    public MassTransitSettings MassTransit { get; init; }
    public bool WaitForInfrastructureOnStartup { get; init; }

    public override string ToString() =>
        new StringBuilder()
            .AppendSettingTitle("Shipment Process")
            .AppendSettingValue(() => ApiBaseUrl)
            .AppendLine()
            .AppendSubSection(() => EventStore)
            .AppendSubSection(() => Postgres)
            .AppendSubSection(() => RabbitMq)
            .AppendSubSection(() => MassTransit)
            .AppendLine()
            .AppendSettingValue(() => WaitForInfrastructureOnStartup)
            .ToString();
}
