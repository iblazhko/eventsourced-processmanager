namespace EventSourcedPM.Messaging.ManifestationAndDocuments.Commands;

using EventSourcedPM.Messaging.Models;

public class CreateShipment : BaseShipmentCommand
{
    public ShipmentLeg[] Legs { get; init; }
}

public class ManifestShipment : BaseShipmentCommand;

public class GenerateShipmentLabels : BaseShipmentCommand;

public class GenerateCustomsInvoice : BaseShipmentCommand;

public class GenerateCombinedDocument : BaseShipmentCommand;

public class GenerateShipmentReceipt : BaseShipmentCommand;
