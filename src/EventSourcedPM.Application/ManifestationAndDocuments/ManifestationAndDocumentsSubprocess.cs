namespace EventSourcedPM.Application.ManifestationAndDocuments;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcedPM.Domain.Aggregates.ManifestationAndDocuments;
using EventSourcedPM.Domain.Models;
using EventSourcedPM.Messaging.ManifestationAndDocuments.Commands;
using EventSourcedPM.Messaging.ManifestationAndDocuments.Events;
using EventSourcedPM.Ports.EventStore;
using Serilog;
using CarrierIntegrationEvents = EventSourcedPM.Ports.CarrierIntegration.Events;

public interface IManifestationAndDocumentsSubprocess
{
    Task Handle(object trigger);
}

public class ManifestationAndDocumentsSubprocess(
    IManifestationAndDocumentsDelegator shipmentDelegator,
    EventSourcedRepository<
        ManifestationAndDocumentsState,
        BaseShipmentEvent
    > manifestationAndDocumentsRepository,
    IEventStreamProjection<ManifestationAndDocumentsState, BaseShipmentEvent> stateProjection
) : IManifestationAndDocumentsSubprocess
{
    public Task Handle(object trigger)
    {
        Log.Information(
            "In {MessageType} trigger handler: {@MessagePayload}",
            trigger.GetType().FullName,
            trigger
        );

        return trigger switch
        {
            CreateShipment x => HandleCreateShipment(x),
            GenerateCustomsInvoice x => HandleGenerateCustomsInvoice(x),
            ManifestShipment x => HandleManifestShipment(x),
            CarrierIntegrationEvents.ShipmentManifestedWithCarrier x
                => HandleCarrierIntegrationShipmentManifestedWithCarrier(x),
            CarrierIntegrationEvents.ShipmentCarrierManifestationFailed x
                => HandleCarrierIntegrationShipmentCarrierManifestationFailed(x),
            ShipmentLegManifested x => HandleShipmentLegManifested(x),
            GenerateShipmentLabels x => HandleGenerateShipmentLabels(x),
            GenerateCombinedDocument x => HandleGenerateCombinedDocument(x),
            GenerateShipmentReceipt x => HandleGenerateShipmentReceipt(x),
            _ => throw new TriggerNotSupportedException(trigger.GetType().FullName)
        };
    }

    private Task HandleCreateShipment(CreateShipment message)
    {
        var shipmentId = (ShipmentId)message.ShipmentId;

        return InvokeAggregate(
            shipmentId,
            _ =>
                ManifestationAndDocumentsAggregate.Create(
                    (ShipmentProcessCategory)message.ProcessCategory,
                    shipmentId,
                    message.Legs?.Select(x => x.ToDomain()).ToArray()
                )
        );
    }

    private Task HandleGenerateCustomsInvoice(GenerateCustomsInvoice message)
    {
        var shipmentId = (ShipmentId)message.ShipmentId;

        return InvokeAggregate(
            shipmentId,
            _ =>
                ManifestationAndDocumentsAggregate.GenerateCustomsInvoice(
                    (ShipmentProcessCategory)message.ProcessCategory,
                    shipmentId
                )
        );
    }

    private Task HandleManifestShipment(ManifestShipment message)
    {
        var shipmentId = (ShipmentId)message.ShipmentId;

        return InvokeAggregate(
            shipmentId,
            state =>
                ManifestationAndDocumentsAggregate.ContinueOrCompleteShipmentManifestation(
                    (ShipmentProcessCategory)message.ProcessCategory,
                    shipmentId,
                    state.Legs,
                    state.ManifestedLegs
                )
        );
    }

    private Task HandleCarrierIntegrationShipmentManifestedWithCarrier(
        CarrierIntegrationEvents.ShipmentManifestedWithCarrier message
    )
    {
        var shipmentId = (ShipmentId)message.ShipmentId;

        return InvokeAggregate(
            shipmentId,
            state =>
                ManifestationAndDocumentsAggregate.SetLegAsManifestedWithCarrier(
                    state.ProcessCategory,
                    shipmentId,
                    message.CarrierId,
                    message.TrackingNumber
                )
        );
    }

    private Task HandleCarrierIntegrationShipmentCarrierManifestationFailed(
        CarrierIntegrationEvents.ShipmentCarrierManifestationFailed message
    )
    {
        var shipmentId = (ShipmentId)message.ShipmentId;

        return InvokeAggregate(
            shipmentId,
            state =>
                ManifestationAndDocumentsAggregate.SetLegAsCarrierManifestationFailed(
                    state.ProcessCategory,
                    shipmentId,
                    message.CarrierId,
                    message.Failure
                )
        );
    }

    private Task HandleShipmentLegManifested(ShipmentLegManifested message)
    {
        var shipmentId = (ShipmentId)message.ShipmentId;

        return InvokeAggregate(
            shipmentId,
            state =>
                ManifestationAndDocumentsAggregate.ContinueOrCompleteShipmentManifestation(
                    (ShipmentProcessCategory)message.ProcessCategory,
                    shipmentId,
                    state.Legs,
                    state.ManifestedLegs
                )
        );
    }

    private Task HandleGenerateShipmentLabels(GenerateShipmentLabels message)
    {
        var shipmentId = (ShipmentId)message.ShipmentId;

        return InvokeAggregate(
            shipmentId,
            state =>
                ManifestationAndDocumentsAggregate.GenerateShipmentLabels(
                    (ShipmentProcessCategory)message.ProcessCategory,
                    shipmentId,
                    state.Legs
                )
        );
    }

    private Task HandleGenerateCombinedDocument(GenerateCombinedDocument message)
    {
        var shipmentId = (ShipmentId)message.ShipmentId;

        return InvokeAggregate(
            shipmentId,
            _ =>
                ManifestationAndDocumentsAggregate.GenerateCombinedDocument(
                    (ShipmentProcessCategory)message.ProcessCategory,
                    shipmentId
                )
        );
    }

    private Task HandleGenerateShipmentReceipt(GenerateShipmentReceipt message)
    {
        var shipmentId = (ShipmentId)message.ShipmentId;

        return InvokeAggregate(
            shipmentId,
            _ =>
                ManifestationAndDocumentsAggregate.GenerateReceipt(
                    (ShipmentProcessCategory)message.ProcessCategory,
                    shipmentId
                )
        );
    }

    private async Task InvokeAggregate(
        ShipmentId shipmentId,
        Func<ManifestationAndDocumentsState, IEnumerable<BaseShipmentEvent>> action
    )
    {
        ManifestationAndDocumentsState manifestationAndDocumentsState = default;
        var newEvents = await manifestationAndDocumentsRepository.Upsert(
            shipmentId.ToEventStreamId(),
            stateProjection,
            state =>
            {
                manifestationAndDocumentsState = state;
                return action(state) ?? Enumerable.Empty<BaseShipmentEvent>();
            }
        );

        var delegatedDecisionsList = newEvents.Where(x => x.Delegated);
        foreach (var decision in delegatedDecisionsList)
        {
            await shipmentDelegator.DelegateDecision(manifestationAndDocumentsState, decision);
        }
    }
}

public class TriggerNotSupportedException(string triggerType)
    : Exception($"Trigger '{triggerType}' is not supported");
