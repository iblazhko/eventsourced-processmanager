using EventSourcedPM.Domain.Models;

namespace EventSourcedPM.Domain.Aggregates.CollectionBooking;

public record CollectionBookingState(
    ShipmentProcessCategory ProcessCategory,
    CollectionBookingId ShipmentId,
    ManifestedShipmentLeg CollectionLeg,
    DateOnly CollectionDate,
    TimeZoneId TimeZone,
    CollectionBookingProcessStatus Status
);
