namespace EventSourcedPM.Domain.Aggregates.Orchestration;

using System;
using EventSourcedPM.Domain.Models;

public record ShipmentProcessState(
    ShipmentProcessCategory Category,
    ShipmentProcessId ShipmentId,
    ShipmentProcessInput ProcessInput,
    ShipmentProcessOutcome ProcessOutcome,
    ShipmentProcessStep ProcessStep,
    ShipmentProcessStagesProgress StagesProgress
);

public record struct ShipmentProcessInput(
    ShipmentLeg[] Legs,
    DateOnly CollectionDate,
    TimeZoneId TimeZone
);

public record struct ShipmentProcessOutcome(
    ManifestedShipmentLeg[] ManifestedLegs,
    ShipmentDocuments Documents,
    string CollectionBookingReference
);

public record struct ShipmentProcessStagesProgress(
    ShipmentProcessStageStatus OverallProcess,
    ShipmentProcessStageStatus ManifestationAndDocuments,
    ShipmentProcessStageStatus ShipmentManifestation,
    ShipmentProcessStageStatus CustomsInvoiceGeneration,
    ShipmentProcessStageStatus ShipmentLabels,
    ShipmentProcessStageStatus CombinedDocument,
    ShipmentProcessStageStatus Receipt,
    ShipmentProcessStageStatus CollectionBooking
);
