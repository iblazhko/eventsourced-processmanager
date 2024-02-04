namespace EventSourcedPM.Domain.Aggregates.ManifestationAndDocuments;

using System;

public record ShipmentDocuments(Uri Labels, Uri Invoice, Uri Receipt, Uri CombinedDocument);
