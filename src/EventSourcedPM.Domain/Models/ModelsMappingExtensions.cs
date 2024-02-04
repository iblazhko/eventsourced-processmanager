namespace EventSourcedPM.Domain.Models;

using System;

public static class ModelMappingExtensions
{
    // ReSharper disable RedundantNameQualifier

    public static Domain.Models.ShipmentLeg ToDomain(this Messaging.Models.ShipmentLeg leg) =>
        new((CarrierId)leg.CarrierId, leg.Sender, leg.Receiver, leg.Collection);

    public static Domain.Models.ManifestedShipmentLeg ToManifestedLeg(
        this Domain.Models.ShipmentLeg leg,
        string trackingNumber,
        Uri labelsDocument = default
    ) =>
        new(
            leg.CarrierId,
            leg.Sender,
            leg.Receiver,
            leg.Collection,
            trackingNumber,
            (DocumentLocation)labelsDocument
        );

    public static Messaging.Models.ShipmentLeg ToDto(this Domain.Models.ShipmentLeg leg) =>
        new()
        {
            CarrierId = (Guid)leg.CarrierId,
            Sender = leg.Sender,
            Receiver = leg.Receiver,
            Collection = leg.Collection
        };

    public static Domain.Models.ManifestedShipmentLeg ToDomain(
        this Messaging.Models.ManifestedShipmentLeg leg
    ) =>
        new(
            (CarrierId)leg.CarrierId,
            leg.Sender,
            leg.Receiver,
            leg.Collection,
            leg.TrackingNumber,
            (DocumentLocation)leg.LabelsDocument
        );

    public static Messaging.Models.ManifestedShipmentLeg ToDto(
        this Domain.Models.ManifestedShipmentLeg leg
    ) =>
        new()
        {
            CarrierId = (Guid)leg.CarrierId,
            Sender = leg.Sender,
            Receiver = leg.Receiver,
            Collection = leg.Collection,
            TrackingNumber = leg.TrackingNumber,
            LabelsDocument = (Uri)leg.LabelsDocument
        };
}
