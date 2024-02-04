// ReSharper disable once CheckNamespace
namespace EventSourcedPM.Messaging.Models;

using System;

public class ShipmentLeg
{
    public Guid CarrierId { get; init; }
    public string Sender { get; init; }
    public string Receiver { get; init; }
    public string Collection { get; init; }
}

public class ManifestedShipmentLeg : ShipmentLeg
{
    public string TrackingNumber { get; init; }
    public Uri LabelsDocument { get; init; }
}

public class ShipmentProcessOutcome
{
    public string ShipmentId { get; init; }
    public string ProcessCategory { get; init; }
    public string TrackingNumbers { get; init; }
    public string CollectionDate { get; init; }
    public string CollectionBookingReference { get; init; }
    public string TimeZone { get; init; }
    public ShipmentDocuments Documents { get; init; }
}

public class ShipmentDocuments
{
    public string Labels { get; init; }
    public string CustomsInvoice { get; init; }
    public string Receipt { get; init; }
    public string CombinedDocument { get; init; }
}
