namespace EventSourcedPM.Domain.Aggregates.ManifestationAndDocuments;

public readonly record struct ShipmentId(string Id)
{
    public static explicit operator string(ShipmentId id) => id.Id;

    public static explicit operator ShipmentId(string id) => new(id);

    public override string ToString() => Id;
}

public static class EventStreamExtensions
{
    public static string ToEventStreamId(this ShipmentId id) => $"{id}-Shipment";
}
