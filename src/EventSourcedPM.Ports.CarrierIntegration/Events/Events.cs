namespace EventSourcedPM.Ports.CarrierIntegration.Events;

using System;

public class ShipmentManifestedWithCarrier
{
    public string ShipmentId { get; init; }
    public Guid CarrierId { get; init; }
    public string TrackingNumber { get; init; }
}

public class ShipmentCarrierManifestationFailed
{
    public string ShipmentId { get; init; }
    public Guid CarrierId { get; init; }
    public string Failure { get; init; }
}

public class CollectionBookedWithCarrier
{
    public string ShipmentId { get; init; }
    public Guid CarrierId { get; init; }
    public string BookingReference { get; init; }
}

public class CarrierCollectionBookingFailed
{
    public string ShipmentId { get; init; }
    public Guid CarrierId { get; init; }
    public string Failure { get; init; }
}
