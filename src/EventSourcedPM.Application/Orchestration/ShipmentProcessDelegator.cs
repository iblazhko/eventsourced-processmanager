namespace EventSourcedPM.Application.Orchestration;

using System.Linq;
using System.Threading.Tasks;
using EventSourcedPM.Domain.Aggregates.Orchestration;
using EventSourcedPM.Domain.Models;
using EventSourcedPM.Messaging.Orchestration.Events;
using EventSourcedPM.Ports.MessageBus;
using Serilog;
using CollectionBookingCommands = EventSourcedPM.Messaging.CollectionBooking.Commands;
using ManifestationAndDocumentsCommands = EventSourcedPM.Messaging.ManifestationAndDocuments.Commands;

public interface IShipmentProcessDelegator
{
    Task DelegateDecision(ShipmentProcessState processState, BaseShipmentProcessEvent decision);
}

public class ShipmentProcessDelegator(IMessageBus messageBus) : IShipmentProcessDelegator
{
    public async Task DelegateDecision(
        ShipmentProcessState processState,
        BaseShipmentProcessEvent decision
    )
    {
        switch (decision)
        {
            case ManifestationAndDocumentsStarted started:
                Log.Information(
                    "Delegating {MessageType} -> {DelegatedMessageType}",
                    typeof(ManifestationAndDocumentsStarted).FullName,
                    typeof(ManifestationAndDocumentsCommands.CreateShipment).FullName
                );
                await messageBus.SendCommand(
                    new ManifestationAndDocumentsCommands.CreateShipment
                    {
                        ShipmentId = started.ShipmentId,
                        ProcessCategory = started.ProcessCategory,
                        Legs = processState.ProcessInput.Legs.Select(x => x.ToDto()).ToArray()
                    }
                );
                break;

            case CustomsInvoiceGenerationStarted invoiceGenerationStarted:
                Log.Information(
                    "Delegating {MessageType} -> {DelegatedMessageType}",
                    typeof(CustomsInvoiceGenerationStarted).FullName,
                    typeof(ManifestationAndDocumentsCommands.GenerateCustomsInvoice).FullName
                );
                await messageBus.SendCommand(
                    new ManifestationAndDocumentsCommands.GenerateCustomsInvoice
                    {
                        ShipmentId = invoiceGenerationStarted.ShipmentId,
                        ProcessCategory = invoiceGenerationStarted.ProcessCategory,
                    }
                );
                break;

            case ShipmentManifestationStarted manifestationStarted:
                Log.Information(
                    "Delegating {MessageType} -> {DelegatedMessageType}",
                    typeof(ShipmentManifestationStarted).FullName,
                    typeof(ManifestationAndDocumentsCommands.ManifestShipment).FullName
                );
                await messageBus.SendCommand(
                    new ManifestationAndDocumentsCommands.ManifestShipment
                    {
                        ShipmentId = manifestationStarted.ShipmentId,
                        ProcessCategory = manifestationStarted.ProcessCategory,
                    }
                );
                break;

            case ShipmentLabelsGenerationStarted labelsGenerationStarted:
                Log.Information(
                    "Delegating {MessageType} -> {DelegatedMessageType}",
                    typeof(ShipmentLabelsGenerationStarted).FullName,
                    typeof(ManifestationAndDocumentsCommands.GenerateShipmentLabels).FullName
                );
                await messageBus.SendCommand(
                    new ManifestationAndDocumentsCommands.GenerateShipmentLabels
                    {
                        ShipmentId = labelsGenerationStarted.ShipmentId,
                        ProcessCategory = labelsGenerationStarted.ProcessCategory,
                    }
                );
                break;

            case ReceiptGenerationStarted receiptGenerationStarted:
                Log.Information(
                    "Delegating {MessageType} -> {DelegatedMessageType}",
                    typeof(ReceiptGenerationStarted).FullName,
                    typeof(ManifestationAndDocumentsCommands.GenerateShipmentReceipt).FullName
                );
                await messageBus.SendCommand(
                    new ManifestationAndDocumentsCommands.GenerateShipmentReceipt
                    {
                        ShipmentId = receiptGenerationStarted.ShipmentId,
                        ProcessCategory = receiptGenerationStarted.ProcessCategory,
                    }
                );
                break;

            case CombinedDocumentGenerationStarted combinedDocumentGenerationStarted:
                Log.Information(
                    "Delegating {MessageType} -> {DelegatedMessageType}",
                    typeof(CombinedDocumentGenerationStarted).FullName,
                    typeof(ManifestationAndDocumentsCommands.GenerateCombinedDocument).FullName
                );
                await messageBus.SendCommand(
                    new ManifestationAndDocumentsCommands.GenerateCombinedDocument
                    {
                        ShipmentId = combinedDocumentGenerationStarted.ShipmentId,
                        ProcessCategory = combinedDocumentGenerationStarted.ProcessCategory,
                    }
                );
                break;

            case CollectionBookingStarted collectionBookingStarted:
                Log.Information(
                    "Delegating {MessageType} -> {DelegatedMessageType}",
                    typeof(CollectionBookingStarted).FullName,
                    typeof(CollectionBookingCommands.CreateCollectionBooking).FullName
                );
                await messageBus.SendCommand(
                    new CollectionBookingCommands.CreateCollectionBooking
                    {
                        ShipmentId = collectionBookingStarted.ShipmentId,
                        ProcessCategory = collectionBookingStarted.ProcessCategory,
                        CollectionLeg = processState.ProcessOutcome.ManifestedLegs.First().ToDto(),
                        CollectionDate = processState.ProcessInput.CollectionDate.ToIsoDate(),
                        TimeZone = (string)processState.ProcessInput.TimeZone
                    }
                );
                break;
        }
    }
}
