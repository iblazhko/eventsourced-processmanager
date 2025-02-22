namespace EventSourcedPM.Application.Orchestration;

using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcedPM.Domain.Aggregates.Orchestration;
using EventSourcedPM.Domain.Models;
using EventSourcedPM.Messaging.Orchestration.Events;

public interface IShipmentProcess
{
    ShipmentProcessCategory Category { get; }

    public IEnumerable<BaseShipmentProcessEvent> Initialize(
        ShipmentProcessId shipmentId,
        ShipmentLeg[] legs,
        DateOnly collectionDate,
        TimeZoneId timeZone
    ) =>
        [
            new ShipmentProcessStarted
            {
                ShipmentId = (string)shipmentId,
                ProcessCategory = (string)Category,
                Legs = legs.Select(x => x.ToDto()).ToArray(),
                CollectionDate = collectionDate.ToIsoDate(),
                TimeZone = (string)timeZone,
            },
        ];

    // Note that BaseShipmentProcessEvent acts both as a record of
    // process manager decision (i.e. what has happened / need to happen
    // given the current state of the process and the incoming trigger),
    // and a trigger itself - decision events will be consumed from the
    // message bus and passed into process so it can make consecutive
    // decisions.
    IEnumerable<BaseShipmentProcessEvent> MakeDecision(ShipmentProcessState shipmentProcessState, ShipmentProcessTrigger trigger);
}
