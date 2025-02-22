namespace EventSourcedPM.Configurators;

using System;
using System.Linq;
using EventSourcedPM.Domain.Aggregates.Orchestration;
using EventSourcedPM.Domain.Models;
using EventSourcedPM.Messaging.Orchestration.Commands;
using EventSourcedPM.Messaging.Orchestration.Events;
using EventSourcedPM.Ports.EventStore;
using EventSourcedPM.Ports.MessageBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MessagingModels = EventSourcedPM.Messaging.Models;

public static class ApiEndpointsConfigurator
{
    private static readonly Guid Carrier1 = Guid.Parse("c62bee76-3e7a-4ce3-87dd-a5eb11678815");
    private static readonly Guid Carrier2 = Guid.Parse("9d6d28f2-fbee-4e53-aeac-2c3b3ba98d28");

    public static void AddApiEndpoints(this WebApplication app)
    {
        var messageBus = app.Services.GetRequiredService<IMessageBus>();
        var shipmentProcessRepository = app.Services.GetRequiredService<EventSourcedRepository<ShipmentProcessState, BaseShipmentProcessEvent>>();
        var shipmentProcessStateProjection = app.Services.GetRequiredService<
            IEventStreamProjection<ShipmentProcessState, BaseShipmentProcessEvent>
        >();

        app.MapGet("/", () => "Hello World!");

        app.MapPost(
            "/{id}",
            async ([FromRoute] string id) =>
            {
                await messageBus.SendCommand(
                    id.StartsWith('1')
                        ? new ProcessShipment
                        {
                            ShipmentId = id,
                            Legs =
                            [
                                new MessagingModels.ShipmentLeg
                                {
                                    CarrierId = Carrier1,
                                    Sender = "GB-sender1",
                                    Receiver = "GB-receiver1",
                                    Collection = "GB-collection1",
                                },
                            ],
                            CollectionDate = DateTime.UtcNow.Date.AddDays(1).ToIsoDate(),
                            TimeZone = "Europe/London",
                        }
                        : new ProcessShipment
                        {
                            ShipmentId = id,
                            Legs =
                            [
                                new MessagingModels.ShipmentLeg
                                {
                                    CarrierId = Carrier1,
                                    Sender = "DE-sender1",
                                    Receiver = "DE-receiver1",
                                    Collection = "DE-collection1",
                                },
                                new MessagingModels.ShipmentLeg
                                {
                                    CarrierId = Carrier2,
                                    Sender = "DE-sender2",
                                    Receiver = "GB-receiver2",
                                    Collection = "DE-collection2",
                                },
                            ],
                            CollectionDate = DateTime.UtcNow.Date.AddDays(1).ToIsoDate(),
                            TimeZone = "Europe/Berlin",
                        }
                );

                return new { ShipmentId = id };
            }
        );

        app.MapGet(
            "/{id}",
            async ([FromRoute] string id) =>
            {
                var processState = await shipmentProcessRepository.GetState(
                    ((ShipmentProcessId)id).ToEventStreamId(),
                    shipmentProcessStateProjection
                );

                return new MessagingModels.ShipmentProcessOutcome
                {
                    ShipmentId = id,
                    ProcessCategory = processState.Category.Id,
                    CollectionDate = processState.ProcessInput.CollectionDate.ToString("yyyy-MM-dd"),
                    TimeZone = processState.ProcessInput.TimeZone.TimeZone,
                    TrackingNumbers = string.Join(
                        ", ",
                        processState.ProcessOutcome.ManifestedLegs?.Select(l => l.TrackingNumber) ?? Array.Empty<string>()
                    ),
                    CollectionBookingReference = processState.ProcessOutcome.CollectionBookingReference,
                    Documents = new MessagingModels.ShipmentDocuments
                    {
                        Labels = processState.ProcessOutcome.Documents?.Labels.ToString(),
                        CustomsInvoice = processState.ProcessOutcome.Documents?.CustomsInvoice.ToString(),
                        Receipt = processState.ProcessOutcome.Documents?.Receipt.ToString(),
                        CombinedDocument = processState.ProcessOutcome.Documents?.CombinedDocument.ToString(),
                    },
                };
            }
        );
    }
}
