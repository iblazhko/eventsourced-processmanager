namespace EventSourcedPM.Domain.Aggregates.ManifestationAndDocuments;

using EventSourcedPM.Domain.Models;

public record ManifestationAndDocumentsState(
    ShipmentProcessCategory ProcessCategory,
    ShipmentId ShipmentId,
    ShipmentLeg[] Legs,
    ManifestedShipmentLeg[] ManifestedLegs,
    ShipmentDocuments Documents
);
