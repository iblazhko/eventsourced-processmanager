namespace EventSourcedPM.Application.ManifestationAndDocuments;

using System.Threading.Tasks;
using EventSourcedPM.Messaging.ManifestationAndDocuments.Commands;
using EventSourcedPM.Messaging.ManifestationAndDocuments.Events;
using MassTransit;
using CarrierIntegrationEvents = EventSourcedPM.Ports.CarrierIntegration.Events;

// ReSharper disable once ClassNeverInstantiated.Global
public class ManifestationAndDocumentsTriggersConsumer(
    IManifestationAndDocumentsSubprocess manifestationAndDocumentsSubprocess
)
    : IConsumer<CreateShipment>,
        IConsumer<GenerateCustomsInvoice>,
        IConsumer<ManifestShipment>,
        IConsumer<CarrierIntegrationEvents.ShipmentManifestedWithCarrier>,
        IConsumer<CarrierIntegrationEvents.ShipmentCarrierManifestationFailed>,
        IConsumer<ShipmentLegManifested>,
        IConsumer<GenerateShipmentLabels>,
        IConsumer<GenerateCombinedDocument>,
        IConsumer<GenerateShipmentReceipt>
{
    public Task Consume(ConsumeContext<CreateShipment> context) =>
        manifestationAndDocumentsSubprocess.Handle(context.Message);

    public Task Consume(ConsumeContext<GenerateCustomsInvoice> context) =>
        manifestationAndDocumentsSubprocess.Handle(context.Message);

    public Task Consume(ConsumeContext<ManifestShipment> context) =>
        manifestationAndDocumentsSubprocess.Handle(context.Message);

    public Task Consume(
        ConsumeContext<CarrierIntegrationEvents.ShipmentManifestedWithCarrier> context
    ) => manifestationAndDocumentsSubprocess.Handle(context.Message);

    public Task Consume(
        ConsumeContext<CarrierIntegrationEvents.ShipmentCarrierManifestationFailed> context
    ) => manifestationAndDocumentsSubprocess.Handle(context.Message);

    public Task Consume(ConsumeContext<ShipmentLegManifested> context) =>
        manifestationAndDocumentsSubprocess.Handle(context.Message);

    public Task Consume(ConsumeContext<GenerateShipmentLabels> context) =>
        manifestationAndDocumentsSubprocess.Handle(context.Message);

    public Task Consume(ConsumeContext<GenerateCombinedDocument> context) =>
        manifestationAndDocumentsSubprocess.Handle(context.Message);

    public Task Consume(ConsumeContext<GenerateShipmentReceipt> context) =>
        manifestationAndDocumentsSubprocess.Handle(context.Message);
}
