using EventSourcedPM.Domain.Models;

namespace EventSourcedPM.Domain.Aggregates.ManifestationAndDocuments;

public record ManifestationAndDocumentsState(
    ShipmentProcessCategory ProcessCategory,
    ShipmentId ShipmentId,
    ShipmentLeg[] Legs,
    ManifestedShipmentLeg[] ManifestedLegs,
    ShipmentDocuments Documents
);
