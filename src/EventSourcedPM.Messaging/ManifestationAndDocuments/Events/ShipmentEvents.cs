namespace EventSourcedPM.Messaging.ManifestationAndDocuments.Events;

using System;
using EventSourcedPM.Messaging.Models;

public class ShipmentInitialized : BaseShipmentEvent
{
    public ShipmentLeg[] Legs { get; init; }
}

public class CustomsInvoiceGenerated : BaseShipmentEvent
{
    public Uri DocumentLocation { get; init; }
}

public class CustomsInvoiceGenerationFailed : BaseShipmentEvent
{
    public string Failure { get; init; }
}

public class ShipmentLegManifestationStarted : BaseShipmentEvent
{
    public Guid CarrierId { get; init; }

    // Alternative (replace above or add for a reference): public int LegPosition { get; init; }
}

public class ShipmentLegManifested : BaseShipmentEvent
{
    public Guid CarrierId { get; init; }

    // Alternative (replace above or add for a reference): public int LegPosition { get; init; }

    public string TrackingNumber { get; init; }
}

public class ShipmentLegManifestationFailed : BaseShipmentEvent
{
    public Guid CarrierId { get; init; }

    // Alternative (replace above or add for a reference): public int LegPosition { get; init; }

    public string Failure { get; init; }
}

public class ShipmentManifested : BaseShipmentEvent
{
    public ManifestedShipmentLeg[] ManifestedLegs { get; set; }
}

public class ShipmentManifestationFailed : BaseShipmentEvent
{
    public string Failure { get; init; }
}

public class ShipmentLabelsGenerated : BaseShipmentEvent
{
    public Uri DocumentLocation { get; init; }
}

public class ShipmentLabelsGenerationFailed : BaseShipmentEvent
{
    public string Failure { get; init; }
}

public class ShipmentReceiptGenerated : BaseShipmentEvent
{
    public Uri DocumentLocation { get; init; }
}

public class ShipmentReceiptGenerationFailed : BaseShipmentEvent
{
    public string Failure { get; init; }
}

public class ShipmentCombinedDocumentGenerated : BaseShipmentEvent
{
    public Uri DocumentLocation { get; init; }
}

public class ShipmentCombinedDocumentGenerationFailed : BaseShipmentEvent
{
    public string Failure { get; init; }
}

public class ShipmentDocumentStored : BaseShipmentEvent
{
    public string DocumentType { get; init; }
    public Uri DocumentLocation { get; init; }
}
