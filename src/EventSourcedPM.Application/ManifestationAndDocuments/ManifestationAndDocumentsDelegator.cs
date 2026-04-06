using EventSourcedPM.Domain.Aggregates.ManifestationAndDocuments;
using EventSourcedPM.Messaging.ManifestationAndDocuments.Events;
using EventSourcedPM.Ports.MessageBus;
using static EventSourcedPM.Application.Orchestration.DelegatorLogger;
using CarrierIntegrationCommands = EventSourcedPM.Ports.CarrierIntegration.Commands;

namespace EventSourcedPM.Application.ManifestationAndDocuments;

public interface IManifestationAndDocumentsDelegator
{
    Task DelegateDecision(ManifestationAndDocumentsState manifestationAndDocumentsState, BaseShipmentEvent decision);
}

public class ManifestationAndDocumentsDelegator(IMessageBus messageBus) : IManifestationAndDocumentsDelegator
{
    public Task DelegateDecision(ManifestationAndDocumentsState manifestationAndDocumentsState, BaseShipmentEvent decision) =>
        decision switch
        {
            ShipmentLegManifestationStarted legManifestationStarted => DelegateManifestShipmentWithCarrier(
                manifestationAndDocumentsState,
                legManifestationStarted
            ),
            _ => Task.CompletedTask,
        };

    private async Task DelegateManifestShipmentWithCarrier(
        ManifestationAndDocumentsState manifestationAndDocumentsState,
        ShipmentLegManifestationStarted legManifestationStarted
    )
    {
        var legToBeManifested = manifestationAndDocumentsState.Legs?.Where(x => x.CarrierId.Id == legManifestationStarted.CarrierId).Single();
        var delegatedMessage = new CarrierIntegrationCommands.ManifestShipmentWithCarrier
        {
            ShipmentId = legManifestationStarted.ShipmentId,
            CarrierId = legManifestationStarted.CarrierId,
            Sender = legToBeManifested?.Sender,
            Receiver = legToBeManifested?.Receiver,
            Collection = legToBeManifested?.Collection,
        };

        LogDelegatingMessage(legManifestationStarted, delegatedMessage);

        await messageBus.SendCommand(delegatedMessage);
    }
}
