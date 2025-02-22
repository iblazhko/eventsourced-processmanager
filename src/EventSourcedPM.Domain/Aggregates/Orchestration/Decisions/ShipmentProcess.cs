namespace EventSourcedPM.Domain.Aggregates.Orchestration.Decisions;

using System.Collections.Generic;
using EventSourcedPM.Messaging.Orchestration.Events;

public static partial class DecideThat
{
    public static IEnumerable<BaseShipmentProcessEvent> ShipmentProcessFailed(CollectionBookingFailed trigger) =>
        [
            new ShipmentProcessFailed
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Failure = trigger.Failure,
            },
        ];

    public static IEnumerable<BaseShipmentProcessEvent> ShipmentProcessFailed(ManifestationAndDocumentsFailed trigger) =>
        [
            new ShipmentProcessFailed
            {
                ProcessCategory = trigger.ProcessCategory,
                ShipmentId = trigger.ShipmentId,
                Failure = trigger.Failure,
            },
        ];

    public static IEnumerable<BaseShipmentProcessEvent> ShipmentProcessMaybeCompleted(CollectionBookingCompleted trigger) =>
        [new ShipmentProcessMaybeCompleted { ProcessCategory = trigger.ProcessCategory, ShipmentId = trigger.ShipmentId }];

    public static IEnumerable<BaseShipmentProcessEvent> ShipmentProcessCompleted(ShipmentProcessMaybeCompleted trigger) =>
        [new ShipmentProcessCompleted { ProcessCategory = trigger.ProcessCategory, ShipmentId = trigger.ShipmentId }];
}
