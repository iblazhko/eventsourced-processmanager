namespace EventSourcedPM.Domain.Aggregates.ManifestationAndDocuments;

using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcedPM.Domain.Models;
using EventSourcedPM.Messaging.ManifestationAndDocuments.Events;

public static class ManifestationAndDocumentsAggregate
{
    public static IEnumerable<BaseShipmentEvent> Create(
        ShipmentProcessCategory processCategory,
        ShipmentId shipmentId,
        ShipmentLeg[] legs
    ) =>
        [
            new ShipmentInitialized
            {
                ShipmentId = (string)shipmentId,
                ProcessCategory = (string)processCategory,
                Legs = legs?.Select(x => x.ToDto()).ToArray()
            }
        ];

    public static IEnumerable<BaseShipmentEvent> GenerateCustomsInvoice(
        ShipmentProcessCategory processCategory,
        ShipmentId shipmentId
    ) =>
        [
            new CustomsInvoiceGenerated
            {
                ShipmentId = (string)shipmentId,
                ProcessCategory = (string)processCategory,
                DocumentLocation = new Uri(
                    $"https://shipment-documents.net/{shipmentId}/customs-invoice"
                )
            }
        ];

    public static IEnumerable<BaseShipmentEvent> SetLegAsManifestedWithCarrier(
        ShipmentProcessCategory processCategory,
        ShipmentId shipmentId,
        Guid carrierId,
        string trackingNumber
    ) =>
        [
            new ShipmentLegManifested
            {
                ShipmentId = (string)shipmentId,
                ProcessCategory = (string)processCategory,
                CarrierId = carrierId,
                TrackingNumber = trackingNumber
            }
        ];

    public static IEnumerable<BaseShipmentEvent> SetLegAsCarrierManifestationFailed(
        ShipmentProcessCategory processCategory,
        ShipmentId shipmentId,
        Guid carrierId,
        string failure
    ) =>
        [
            new ShipmentLegManifestationFailed
            {
                ShipmentId = (string)shipmentId,
                ProcessCategory = (string)processCategory,
                CarrierId = carrierId,
                Failure = failure
            },
            new ShipmentManifestationFailed
            {
                ShipmentId = (string)shipmentId,
                ProcessCategory = (string)processCategory,
                Failure = failure
            },
        ];

    public static IEnumerable<BaseShipmentEvent> ContinueOrCompleteShipmentManifestation(
        ShipmentProcessCategory processCategory,
        ShipmentId shipmentId,
        ShipmentLeg[] legs,
        ManifestedShipmentLeg[] manifestedLegs
    )
    {
        var legsToBeManifested = legs.Where(l =>
                manifestedLegs.All(m => m.CarrierId != l.CarrierId)
            )
            .ToList();
        return
        [
            legsToBeManifested.Count == 0
                ? new ShipmentManifested
                {
                    ProcessCategory = (string)processCategory,
                    ShipmentId = (string)shipmentId,
                    ManifestedLegs = manifestedLegs.Select(x => x.ToDto()).ToArray()
                }
                : new ShipmentLegManifestationStarted
                {
                    ProcessCategory = (string)processCategory,
                    ShipmentId = (string)shipmentId,
                    CarrierId = (Guid)legsToBeManifested.First().CarrierId,
                    Delegated = true
                }
        ];
    }

    public static IEnumerable<BaseShipmentEvent> GenerateShipmentLabels(
        ShipmentProcessCategory processCategory,
        ShipmentId shipmentId,
        ShipmentLeg[] legs
    ) =>
        [
            new ShipmentLabelsGenerated
            {
                ShipmentId = (string)shipmentId,
                ProcessCategory = (string)processCategory,
                DocumentLocation = new Uri($"https://shipment-documents.net/{shipmentId}/labels")
            }
        ];

    public static IEnumerable<BaseShipmentEvent> GenerateReceipt(
        ShipmentProcessCategory processCategory,
        ShipmentId shipmentId
    ) =>
        [
            new ShipmentReceiptGenerated
            {
                ShipmentId = (string)shipmentId,
                ProcessCategory = (string)processCategory,
                DocumentLocation = new Uri($"https://shipment-documents.net/{shipmentId}/receipt")
            }
        ];

    public static IEnumerable<BaseShipmentEvent> GenerateCombinedDocument(
        ShipmentProcessCategory processCategory,
        ShipmentId shipmentId
    ) =>
        [
            new ShipmentCombinedDocumentGenerated
            {
                ShipmentId = (string)shipmentId,
                ProcessCategory = (string)processCategory,
                DocumentLocation = new Uri(
                    $"https://shipment-documents.net/{shipmentId}/combined-document"
                )
            }
        ];
}
