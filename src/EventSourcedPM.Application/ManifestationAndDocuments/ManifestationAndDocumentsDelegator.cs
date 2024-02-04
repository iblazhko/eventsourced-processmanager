namespace EventSourcedPM.Application.ManifestationAndDocuments;

using System.Linq;
using System.Threading.Tasks;
using EventSourcedPM.Domain.Aggregates.ManifestationAndDocuments;
using EventSourcedPM.Messaging.ManifestationAndDocuments.Events;
using EventSourcedPM.Ports.MessageBus;
using Serilog;
using CarrierIntegrationCommands = EventSourcedPM.Ports.CarrierIntegration.Commands;

public interface IManifestationAndDocumentsDelegator
{
    Task DelegateDecision(
        ManifestationAndDocumentsState manifestationAndDocumentsState,
        BaseShipmentEvent decision
    );
}

public class ManifestationAndDocumentsDelegator(IMessageBus messageBus)
    : IManifestationAndDocumentsDelegator
{
    public async Task DelegateDecision(
        ManifestationAndDocumentsState manifestationAndDocumentsState,
        BaseShipmentEvent decision
    )
    {
        switch (decision)
        {
            case ShipmentLegManifestationStarted legManifestationStarted:
                Log.Information(
                    "Delegating {MessageType} -> {DelegatedMessageType}",
                    typeof(ShipmentLegManifestationStarted).FullName,
                    typeof(CarrierIntegrationCommands.ManifestShipmentWithCarrier).FullName
                );
                var legToBeManifested = manifestationAndDocumentsState
                    .Legs?.Where(x => x.CarrierId.Id == legManifestationStarted.CarrierId)
                    .Single();

                await messageBus.SendCommand(
                    new CarrierIntegrationCommands.ManifestShipmentWithCarrier
                    {
                        ShipmentId = legManifestationStarted.ShipmentId,
                        CarrierId = legManifestationStarted.CarrierId,
                        Sender = legToBeManifested?.Sender,
                        Receiver = legToBeManifested?.Receiver,
                        Collection = legToBeManifested?.Collection
                    }
                );
                break;
        }
    }
}
