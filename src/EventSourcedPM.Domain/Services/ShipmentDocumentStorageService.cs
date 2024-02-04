namespace EventSourcedPM.Domain.Services;

using System.Threading.Tasks;
using EventSourcedPM.Domain.Aggregates.ManifestationAndDocuments;
using EventSourcedPM.Domain.Models;

public interface IStoreShipmentDocument
{
    DocumentLocation GetDocumentLocation(ShipmentId shipmentId, ShipmentDocumentType documentType);

    Task<DocumentLocation> StoreShipmentDocument(
        ShipmentId shipmentId,
        ShipmentDocumentType documentType,
        DocumentContent content
    );

    Task<DocumentContent> ReadShipmentDocument(
        ShipmentId shipmentId,
        ShipmentDocumentType documentType
    );
}

public class ShipmentDocumentStorageService : IStoreShipmentDocument
{
    public DocumentLocation GetDocumentLocation(
        ShipmentId shipmentId,
        ShipmentDocumentType documentType
    )
    {
        throw new System.NotImplementedException();
    }

    public Task<DocumentLocation> StoreShipmentDocument(
        ShipmentId shipmentId,
        ShipmentDocumentType documentType,
        DocumentContent content
    )
    {
        throw new System.NotImplementedException();
    }

    public Task<DocumentContent> ReadShipmentDocument(
        ShipmentId shipmentId,
        ShipmentDocumentType documentType
    )
    {
        throw new System.NotImplementedException();
    }
}
