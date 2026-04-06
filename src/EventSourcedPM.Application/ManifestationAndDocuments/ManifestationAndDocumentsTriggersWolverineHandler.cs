using EventSourcedPM.Messaging.ManifestationAndDocuments.Commands;
using EventSourcedPM.Messaging.ManifestationAndDocuments.Events;
using CarrierIntegrationEvents = EventSourcedPM.Ports.CarrierIntegration.Events;

namespace EventSourcedPM.Application.ManifestationAndDocuments;

// ReSharper disable once ClassNeverInstantiated.Global
public class ManifestationAndDocumentsTriggersWolverineHandler(IManifestationAndDocumentsSubprocess manifestationAndDocumentsSubprocess)
{
    public Task Handle(CreateShipment message) => manifestationAndDocumentsSubprocess.Handle(message);

    public Task Handle(GenerateCustomsInvoice message) => manifestationAndDocumentsSubprocess.Handle(message);

    public Task Handle(ManifestShipment message) => manifestationAndDocumentsSubprocess.Handle(message);

    public Task Handle(CarrierIntegrationEvents.ShipmentManifestedWithCarrier message) => manifestationAndDocumentsSubprocess.Handle(message);

    public Task Handle(CarrierIntegrationEvents.ShipmentCarrierManifestationFailed message) => manifestationAndDocumentsSubprocess.Handle(message);

    public Task Handle(ShipmentLegManifested message) => manifestationAndDocumentsSubprocess.Handle(message);

    public Task Handle(GenerateShipmentLabels message) => manifestationAndDocumentsSubprocess.Handle(message);

    public Task Handle(GenerateCombinedDocument message) => manifestationAndDocumentsSubprocess.Handle(message);

    public Task Handle(GenerateShipmentReceipt message) => manifestationAndDocumentsSubprocess.Handle(message);
}
