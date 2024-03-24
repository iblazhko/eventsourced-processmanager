namespace EventSourcedPM.Application.Orchestration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcedPM.Domain.Aggregates.Orchestration;
using EventSourcedPM.Domain.Models;
using EventSourcedPM.Messaging;
using EventSourcedPM.Messaging.Orchestration.Commands;
using EventSourcedPM.Messaging.Orchestration.Events;
using EventSourcedPM.Ports.EventStore;
using Serilog;

public interface IShipmentProcessManager
{
    Task InitializeProcess(
        ProcessShipment message,
        Guid? correlationId = default,
        Guid? causationId = default
    );

    Task InvokeProcessTrigger<T>(
        T trigger,
        Guid? correlationId = default,
        Guid? causationId = default
    )
        where T : BaseShipmentWithProcessCategoryEvent;
}

public class ShipmentProcessManager(
    IClassifyShipmentProcess processClassifier,
    IShipmentProcessRegistry processRegistry,
    IShipmentProcessDelegator processDelegator,
    EventSourcedRepository<
        ShipmentProcessState,
        BaseShipmentProcessEvent
    > shipmentProcessRepository,
    IEventStreamProjection<ShipmentProcessState, BaseShipmentProcessEvent> stateProjection
) : IShipmentProcessManager
{
    // Entry point for the shipment process.
    //
    // Here we need to classify the incoming ProcessShipment i.e. determine process category,
    // to find out what kind of process to run. This process category will be carried over
    // in all the following shipment process messages related to this shipment.
    public async Task InitializeProcess(
        ProcessShipment message,
        Guid? correlationId = default,
        Guid? causationId = default
    )
    {
        Log.Information(
            "In {MessageType} consumer: {@MessagePayload}",
            message.GetType().FullName,
            message
        );

        var processCategory = processClassifier.ClassifyShipment(message);
        Log.Information(
            "Shipment {ShipmentId} will use process category '{ProcessCategory}'",
            message.ShipmentId,
            processCategory
        );
        var process = processRegistry.GetByCategory(processCategory);

        await InvokeAggregate(
            (ShipmentProcessId)message.ShipmentId,
            _ =>
                process.Initialize(
                    (ShipmentProcessId)message.ShipmentId,
                    message.Legs.Select(x => x.ToDomain()).ToArray(),
                    DateOnly.TryParse(message.CollectionDate, out var d)
                        ? d
                        : DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
                    (TimeZoneId)message.TimeZone
                ),
            correlationId,
            causationId
        );
    }

    public async Task InvokeProcessTrigger<T>(
        T trigger,
        Guid? correlationId = default,
        Guid? causationId = default
    )
        where T : BaseShipmentWithProcessCategoryEvent
    {
        Log.Information(
            "In {MessageType} consumer: {@MessagePayload}",
            trigger.GetType().FullName,
            trigger
        );

        var processCategory = (ShipmentProcessCategory)trigger.ProcessCategory;
        var process = processRegistry.GetByCategory(processCategory);

        await InvokeAggregate(
            (ShipmentProcessId)trigger.ShipmentId,
            shipmentProcessState =>
                process.MakeDecision(
                    shipmentProcessState,
                    ShipmentProcessTrigger.FromEvent(trigger)
                ),
            correlationId,
            causationId
        );
    }

    private async Task InvokeAggregate(
        ShipmentProcessId shipmentId,
        Func<ShipmentProcessState, IEnumerable<BaseShipmentProcessEvent>> action,
        Guid? correlationId = default,
        Guid? causationId = default
    )
    {
        ShipmentProcessState processState = default;
        var decisions = await shipmentProcessRepository.Upsert(
            shipmentId.ToEventStreamId(),
            stateProjection,
            state =>
            {
                processState = state;
                return action(state) ?? [];
            },
            correlationId,
            causationId
        );

        var delegatedDecisionsList = decisions.Where(x => x.Delegated);
        foreach (var delegatedDecision in delegatedDecisionsList)
        {
            // TODO: make it more clear if we are using process state *before* making decision or *after*
            // With current implementation we are implicitly using state *after* making the decision
            await processDelegator.DelegateDecision(processState, delegatedDecision);
        }
    }
}
