namespace EventSourcedPM.Domain.Aggregates.Orchestration;

public readonly record struct ShipmentProcessId(string Id)
{
    public static explicit operator string(ShipmentProcessId id) => id.Id;

    public static explicit operator ShipmentProcessId(string id) => new(id);

    public override string ToString() => Id;
}

public static class EventStreamExtensions
{
    public static string ToEventStreamId(this ShipmentProcessId id) => $"Process_{id}";
}
