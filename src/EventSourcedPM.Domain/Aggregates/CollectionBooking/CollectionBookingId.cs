namespace EventSourcedPM.Domain.Aggregates.CollectionBooking;

public readonly record struct CollectionBookingId(string Id)
{
    public static explicit operator string(CollectionBookingId id) => id.Id;

    public static explicit operator CollectionBookingId(string id) => new(id);

    public override string ToString() => Id;
}

public static class EventStreamExtensions
{
    public static string ToEventStreamId(this CollectionBookingId id) => $"Collection_{id}";
}
