namespace EventSourcedPM.Domain.Aggregates.ManifestationAndDocuments;

public record ShipmentDocuments(Uri Labels, Uri Invoice, Uri Receipt, Uri CombinedDocument);
