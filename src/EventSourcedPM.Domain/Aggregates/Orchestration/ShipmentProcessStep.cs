namespace EventSourcedPM.Domain.Aggregates.Orchestration;

public enum ShipmentProcessStep
{
    ShipmentProcessStarted,
    ShipmentProcessCompleted,
    ShipmentProcessFailed,
    ManifestationAndDocumentsStarted,
    ManifestationAndDocumentsCompleted,
    ManifestationAndDocumentsFailed,
    ShipmentManifestationStarted,
    ShipmentManifestationCompleted,
    ShipmentManifestationFailed,
    ShipmentLabelsGenerationStarted,
    ShipmentLabelsGenerationCompleted,
    ShipmentLabelsGenerationFailed,
    CustomsInvoiceGenerationStarted,
    CustomsInvoiceGenerationCompleted,
    CustomsInvoiceGenerationFailed,
    ReceiptGenerationStarted,
    ReceiptGenerationCompleted,
    ReceiptGenerationFailed,
    CombinedDocumentGenerationStarted,
    CombinedDocumentGenerationCompleted,
    CombinedDocumentGenerationFailed,
    CollectionBookingStarted,
    CollectionBookingCompleted,
    CollectionBookingFailed,
}
