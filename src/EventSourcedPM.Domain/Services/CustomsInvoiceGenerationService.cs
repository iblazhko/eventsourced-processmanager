namespace EventSourcedPM.Domain.Services;

using System;
using System.Threading.Tasks;
using EventSourcedPM.Messaging.Models;

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
