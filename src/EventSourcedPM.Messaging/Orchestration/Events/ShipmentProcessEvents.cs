namespace EventSourcedPM.Messaging.Orchestration.Events;

using System;
using EventSourcedPM.Messaging.Models;

// Overall shipment process
public class ShipmentProcessStarted : BaseShipmentProcessEvent
{
    // Process input
    // Relevant parts will be forwarded to
    // "Manifestation and Documents" aggregate and "Collection Booking" aggregate
    public ShipmentLeg[] Legs { get; init; }
    public string CollectionDate { get; init; }
    public string TimeZone { get; init; }
}

public class ShipmentProcessCompletionChecked : BaseShipmentProcessEvent;

public class ShipmentProcessCompleted : BaseShipmentProcessEvent;

public class ShipmentProcessFailed : BaseShipmentProcessEvent
{
    public string Failure { get; init; }
}

// Manifestation and documents stage

public class ManifestationAndDocumentsStarted : BaseShipmentProcessEvent;

public class ManifestationAndDocumentsCompleted : BaseShipmentProcessEvent;

public class ManifestationAndDocumentsFailed : BaseShipmentProcessEvent
{
    public string Failure { get; init; }
}

// Manifestation and documents sub-process events
public class CustomsInvoiceGenerationStarted : BaseShipmentProcessEvent;

public class CustomsInvoiceGenerationCompleted : BaseShipmentProcessEvent
{
    public Uri CustomsInvoice { get; set; }
}

public class CustomsInvoiceGenerationFailed : BaseShipmentProcessEvent
{
    public string Failure { get; init; }
}

public class ShipmentManifestationStarted : BaseShipmentProcessEvent;

public class ShipmentManifestationCompleted : BaseShipmentProcessEvent
{
    public ManifestedShipmentLeg[] ManifestedLegs { get; set; }
}

public class ShipmentManifestationFailed : BaseShipmentProcessEvent
{
    public string Failure { get; init; }
}

public class ShipmentLabelsGenerationStarted : BaseShipmentProcessEvent;

public class ShipmentLabelsGenerationCompleted : BaseShipmentProcessEvent
{
    public Uri ShipmentLabels { get; set; }
}

public class ShipmentLabelsGenerationFailed : BaseShipmentProcessEvent
{
    public string Failure { get; init; }
}

public class ReceiptGenerationStarted : BaseShipmentProcessEvent;

public class ReceiptGenerationCompleted : BaseShipmentProcessEvent
{
    public Uri Receipt { get; set; }
}

public class ReceiptGenerationFailed : BaseShipmentProcessEvent
{
    public string Failure { get; init; }
}

public class CombinedDocumentGenerationStarted : BaseShipmentProcessEvent;

public class CombinedDocumentGenerationCompleted : BaseShipmentProcessEvent
{
    public Uri CombinedDocument { get; set; }
}

public class CombinedDocumentGenerationFailed : BaseShipmentProcessEvent
{
    public string Failure { get; init; }
}

// Collection booking process stage

public class CollectionBookingStarted : BaseShipmentProcessEvent;

public class CollectionBookingCompleted : BaseShipmentProcessEvent
{
    public string BookingReference { get; set; }
}

public class CollectionBookingFailed : BaseShipmentProcessEvent
{
    public string Failure { get; init; }
}
