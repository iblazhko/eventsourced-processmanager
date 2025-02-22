namespace EventSourcedPM.Application.Orchestration;

using System;
using System.Collections.Generic;
using EventSourcedPM.Application.Orchestration.Processes;
using EventSourcedPM.Domain.Models;

public interface IShipmentProcessRegistry
{
    IShipmentProcess GetByCategory(ShipmentProcessCategory category);
}

public class ShipmentProcessRegistry : IShipmentProcessRegistry
{
    private static readonly Dictionary<ShipmentProcessCategory, IShipmentProcess> RegisteredProcesses = new()
    {
        { DefaultShipmentProcess.ShipmentProcessCategory, new DefaultShipmentProcess() },
        { DomesticShipmentProcessV1.ShipmentProcessCategory, new DomesticShipmentProcessV1() },
        { InternationalShipmentProcessV1.ShipmentProcessCategory, new InternationalShipmentProcessV1() },
    };

    public IShipmentProcess GetByCategory(ShipmentProcessCategory processCategory) =>
        RegisteredProcesses.TryGetValue(processCategory, out var process) ? process : throw new ProcessNotSupportedException(processCategory);
}

public class ProcessNotSupportedException(ShipmentProcessCategory processCategory) : Exception($"Process '{processCategory}' is not supported");
