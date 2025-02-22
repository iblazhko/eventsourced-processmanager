namespace EventSourcedPM.Application.Orchestration;

using System.Linq;
using System.Threading.Tasks;
using EventSourcedPM.Domain.Aggregates.Orchestration;
using EventSourcedPM.Domain.Models;
using EventSourcedPM.Messaging.Orchestration.Events;
using EventSourcedPM.Ports.MessageBus;
using static EventSourcedPM.Application.Orchestration.DelegatorLogger;
using CollectionBookingCommands = EventSourcedPM.Messaging.CollectionBooking.Commands;
using ManifestationAndDocumentsCommands = EventSourcedPM.Messaging.ManifestationAndDocuments.Commands;

public interface IShipmentProcessDelegator
{
    Task DelegateDecision(ShipmentProcessState processState, BaseShipmentProcessEvent decision);
}

public class ShipmentProcessDelegator(IMessageBus messageBus) : IShipmentProcessDelegator
{
    public Task DelegateDecision(ShipmentProcessState processState, BaseShipmentProcessEvent decision) =>
        decision switch
        {
            ManifestationAndDocumentsStarted started => DelegateCreateShipment(processState, started),

            CustomsInvoiceGenerationStarted invoiceGenerationStarted => DelegateGenerateCustomsInvoice(invoiceGenerationStarted),

            ShipmentManifestationStarted manifestationStarted => DelegateManifestShipment(manifestationStarted),

            ShipmentLabelsGenerationStarted labelsGenerationStarted => DelegateGenerateShipmentLabels(labelsGenerationStarted),

            ReceiptGenerationStarted receiptGenerationStarted => DelegateGenerateShipmentReceipt(receiptGenerationStarted),

            CombinedDocumentGenerationStarted combinedDocumentGenerationStarted => DelegateGenerateCombinedDocuments(
                combinedDocumentGenerationStarted
            ),

            CollectionBookingStarted collectionBookingStarted => DelegateCreateCollectionBooking(processState, collectionBookingStarted),
            _ => Task.CompletedTask,
        };

    private async Task DelegateCreateShipment(ShipmentProcessState processState, ManifestationAndDocumentsStarted started)
    {
        var delegatedMessage = new ManifestationAndDocumentsCommands.CreateShipment
        {
            ShipmentId = started.ShipmentId,
            ProcessCategory = started.ProcessCategory,
            Legs = processState.ProcessInput.Legs.Select(x => x.ToDto()).ToArray(),
        };

        LogDelegatingMessage(started, delegatedMessage);

        await messageBus.SendCommand(delegatedMessage);
    }

    private async Task DelegateGenerateCustomsInvoice(CustomsInvoiceGenerationStarted invoiceGenerationStarted)
    {
        var delegatedMessage = new ManifestationAndDocumentsCommands.GenerateCustomsInvoice
        {
            ShipmentId = invoiceGenerationStarted.ShipmentId,
            ProcessCategory = invoiceGenerationStarted.ProcessCategory,
        };

        LogDelegatingMessage(invoiceGenerationStarted, delegatedMessage);

        await messageBus.SendCommand(delegatedMessage);
    }

    private async Task DelegateManifestShipment(ShipmentManifestationStarted manifestationStarted)
    {
        var delegatedMessage = new ManifestationAndDocumentsCommands.ManifestShipment
        {
            ShipmentId = manifestationStarted.ShipmentId,
            ProcessCategory = manifestationStarted.ProcessCategory,
        };

        LogDelegatingMessage(manifestationStarted, delegatedMessage);

        await messageBus.SendCommand(delegatedMessage);
    }

    private async Task DelegateGenerateShipmentReceipt(ReceiptGenerationStarted receiptGenerationStarted)
    {
        var delegatedMessage = new ManifestationAndDocumentsCommands.GenerateShipmentReceipt
        {
            ShipmentId = receiptGenerationStarted.ShipmentId,
            ProcessCategory = receiptGenerationStarted.ProcessCategory,
        };

        LogDelegatingMessage(receiptGenerationStarted, delegatedMessage);

        await messageBus.SendCommand(delegatedMessage);
    }

    private async Task DelegateCreateCollectionBooking(ShipmentProcessState processState, CollectionBookingStarted collectionBookingStarted)
    {
        if (processState.ProcessOutcome.ManifestedLegs.Length == 0)
        {
            const string reason = "collection booking started with no manifested legs";
            LogCannotelegateMessage<CollectionBookingStarted, CollectionBookingCommands.CreateCollectionBooking>(reason);
            throw new ConcurrencyException((string)processState.ShipmentId, reason);
        }

        var delegatedMessage = new CollectionBookingCommands.CreateCollectionBooking
        {
            ShipmentId = collectionBookingStarted.ShipmentId,
            ProcessCategory = collectionBookingStarted.ProcessCategory,
            CollectionLeg = processState.ProcessOutcome.ManifestedLegs.First().ToDto(),
            CollectionDate = processState.ProcessInput.CollectionDate.ToIsoDate(),
            TimeZone = (string)processState.ProcessInput.TimeZone,
        };

        LogDelegatingMessage(collectionBookingStarted, delegatedMessage);

        await messageBus.SendCommand(delegatedMessage);
    }

    private async Task DelegateGenerateCombinedDocuments(CombinedDocumentGenerationStarted combinedDocumentGenerationStarted)
    {
        var delegatedMessage = new ManifestationAndDocumentsCommands.GenerateCombinedDocument
        {
            ShipmentId = combinedDocumentGenerationStarted.ShipmentId,
            ProcessCategory = combinedDocumentGenerationStarted.ProcessCategory,
        };

        LogDelegatingMessage(combinedDocumentGenerationStarted, delegatedMessage);

        await messageBus.SendCommand(delegatedMessage);
    }

    private async Task DelegateGenerateShipmentLabels(ShipmentLabelsGenerationStarted labelsGenerationStarted)
    {
        var delegatedMessage = new ManifestationAndDocumentsCommands.GenerateShipmentLabels
        {
            ShipmentId = labelsGenerationStarted.ShipmentId,
            ProcessCategory = labelsGenerationStarted.ProcessCategory,
        };

        LogDelegatingMessage(labelsGenerationStarted, delegatedMessage);

        await messageBus.SendCommand(delegatedMessage);
    }
}
