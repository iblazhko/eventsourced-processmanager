using EventSourcedPM.Messaging.Models;

namespace EventSourcedPM.Domain.Services;

public interface IGenerateCustomsInvoice
{
    Task<Uri> GenerateCustomsInvoice(ManifestedShipmentLeg leg);
}

public class CustomsInvoiceGenerationService : IGenerateCustomsInvoice
{
    public Task<Uri> GenerateCustomsInvoice(ManifestedShipmentLeg leg)
    {
        throw new NotImplementedException();
    }
}
