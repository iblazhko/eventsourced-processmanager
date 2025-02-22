namespace EventSourcedPM.Domain.Aggregates.ManifestationAndDocuments;

using System;
using System.Linq;
using EventSourcedPM.Domain.Models;
using EventSourcedPM.Messaging.ManifestationAndDocuments.Events;
using EventSourcedPM.Ports.EventStore;

public class ManifestationAndDocumentsStateProjection : IEventStreamProjection<ManifestationAndDocumentsState, BaseShipmentEvent>
{
    public ManifestationAndDocumentsState GetInitialState(EventStreamId streamId) =>
        new(
            default,
            default,
            Array.Empty<ShipmentLeg>(),
            Array.Empty<ManifestedShipmentLeg>(),
            new ShipmentDocuments(default, default, default, default)
        );

    public ManifestationAndDocumentsState Apply(ManifestationAndDocumentsState state, BaseShipmentEvent evt) =>
        evt switch
        {
            ShipmentInitialized x => new(
                (ShipmentProcessCategory)x.ProcessCategory,
                (ShipmentId)x.ShipmentId,
                x.Legs?.Select(l => l.ToDomain()).ToArray(),
                Array.Empty<ManifestedShipmentLeg>(),
                new ShipmentDocuments(default, default, default, default)
            ),
            ShipmentLegManifestationStarted => state,
            ShipmentLegManifested x => state with
            {
                ManifestedLegs = state
                    .ManifestedLegs.Where(l => l.CarrierId.Id != x.CarrierId)
                    .Append(state.Legs.Where(l => l.CarrierId.Id == x.CarrierId).Select(l => l.ToManifestedLeg(x.TrackingNumber)).Single())
                    .ToArray(),
            },
            ShipmentLegManifestationFailed => state,
            ShipmentManifestationFailed => state,
            ShipmentManifested => state,
            ShipmentLabelsGenerated x => state with { Documents = state.Documents with { Labels = x.DocumentLocation } },
            CustomsInvoiceGenerated x => state with { Documents = state.Documents with { Invoice = x.DocumentLocation } },
            ShipmentReceiptGenerated x => state with { Documents = state.Documents with { Receipt = x.DocumentLocation } },
            ShipmentCombinedDocumentGenerated x => state with { Documents = state.Documents with { CombinedDocument = x.DocumentLocation } },
            _ => state,
        };
}
