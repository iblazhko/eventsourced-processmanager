namespace EventSourcedPM.Domain.Aggregates.CollectionBooking;

using System;
using EventSourcedPM.Domain.Models;

public record CollectionBookingState(
    ShipmentProcessCategory ProcessCategory,
    CollectionBookingId ShipmentId,
    ManifestedShipmentLeg CollectionLeg,
    DateOnly CollectionDate,
    TimeZoneId TimeZone,
    CollectionBookingProcessStatus Status
);
