namespace EventSourcedPM.Ports.CarrierIntegration.Commands;

using System;

public class ManifestShipmentWithCarrier
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    public string ShipmentId { get; init; }
    public Guid CarrierId { get; init; }
    public string Sender { get; init; }
    public string Receiver { get; init; }
    public string Collection { get; init; }
}
