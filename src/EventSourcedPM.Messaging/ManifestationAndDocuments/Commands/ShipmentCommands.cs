using EventSourcedPM.Messaging.Models;

namespace EventSourcedPM.Messaging.ManifestationAndDocuments.Commands;

public class CreateShipment : BaseShipmentCommand
{
    public ShipmentLeg[] Legs { get; init; }
}

public class ManifestShipment : BaseShipmentCommand;

public class GenerateShipmentLabels : BaseShipmentCommand;

public class GenerateCustomsInvoice : BaseShipmentCommand;

public class GenerateCombinedDocument : BaseShipmentCommand;

public class GenerateShipmentReceipt : BaseShipmentCommand;
