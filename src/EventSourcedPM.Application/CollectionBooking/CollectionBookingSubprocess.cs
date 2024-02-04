namespace EventSourcedPM.Application.CollectionBooking;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcedPM.Domain.Aggregates.CollectionBooking;
using EventSourcedPM.Domain.Models;
using EventSourcedPM.Domain.Services;
using EventSourcedPM.Messaging.CollectionBooking.Commands;
using EventSourcedPM.Messaging.CollectionBooking.Events;
using EventSourcedPM.Ports.EventStore;
using EventSourcedPM.Ports.MessageBus;
using Serilog;
using CarrierIntegrationEvents = EventSourcedPM.Ports.CarrierIntegration.Events;

public interface ICollectionBookingSubprocess
{
    Task Handle(object trigger);
}

public class CollectionBookingSubprocess(
    EventSourcedRepository<
        CollectionBookingState,
        BaseCollectionBookingEvent
    > collectionBookingRepository,
    IEventStreamProjection<CollectionBookingState, BaseCollectionBookingEvent> stateProjection,
    ICollectionBookingDelegator collectionBookingDelegator,
    ICollectionBookingScheduler collectionBookingScheduler,
    IMessageBus messageBus
) : ICollectionBookingSubprocess
{
    public Task Handle(object trigger)
    {
        Log.Information(
            "In {MessageType} trigger handler: {@MessagePayload}",
            trigger.GetType().FullName,
            trigger
        );

        return trigger switch
        {
            CreateCollectionBooking x => HandleCreateCollectionBooking(x),
            ScheduleCollectionBooking x => HandleScheduleCollectionBooking(x),
            BookCollectionWithCarrier x => HandleBookCollectionWithCarrier(x),
            CarrierIntegrationEvents.CollectionBookedWithCarrier x
                => HandleCarrierIntegrationCollectionBookedWithCarrier(x),
            CarrierIntegrationEvents.CarrierCollectionBookingFailed x
                => HandleCarrierIntegrationCarrierCollectionBookingFailed(x),
            _ => throw new TriggerNotSupportedException(trigger.GetType().FullName)
        };
    }

    private async Task HandleCreateCollectionBooking(CreateCollectionBooking message)
    {
        var shipmentId = (CollectionBookingId)message.ShipmentId;

        await InvokeAggregate(
            shipmentId,
            _ =>
                CollectionBookingAggregate.Create(
                    (ShipmentProcessCategory)message.ProcessCategory,
                    shipmentId,
                    message.CollectionLeg.ToDomain(),
                    DateOnly.TryParse(message.CollectionDate, out var d)
                        ? d
                        : DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1).Date),
                    (TimeZoneId)message.TimeZone
                )
        );

        // For testing purposes, trigger scheduling and booking in-place

        await Task.Delay(TimeSpan.FromMilliseconds(200));
        await messageBus.SendCommand(
            new ScheduleCollectionBooking
            {
                ProcessCategory = message.ProcessCategory,
                ShipmentId = message.ShipmentId,
                CollectionDate = message.CollectionDate
            }
        );

        await Task.Delay(TimeSpan.FromSeconds(5));
        await messageBus.SendCommand(
            new BookCollectionWithCarrier
            {
                ProcessCategory = message.ProcessCategory,
                ShipmentId = message.ShipmentId,
            }
        );
    }

    private Task HandleScheduleCollectionBooking(ScheduleCollectionBooking message)
    {
        var shipmentId = (CollectionBookingId)message.ShipmentId;

        return InvokeAggregate(
            shipmentId,
            state =>
                CollectionBookingAggregate.ScheduleCollectionBooking(
                    (ShipmentProcessCategory)message.ProcessCategory,
                    state,
                    DateOnly.TryParse(message.CollectionDate, out var date)
                        ? date
                        : DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
                    collectionBookingScheduler
                )
        );
    }

    private Task HandleBookCollectionWithCarrier(BookCollectionWithCarrier message)
    {
        var shipmentId = (CollectionBookingId)message.ShipmentId;

        return InvokeAggregate(
            shipmentId,
            state =>
                CollectionBookingAggregate.BookCollectionWithCarrier(
                    (ShipmentProcessCategory)message.ProcessCategory,
                    state
                )
        );
    }

    private Task HandleCarrierIntegrationCollectionBookedWithCarrier(
        CarrierIntegrationEvents.CollectionBookedWithCarrier message
    )
    {
        var shipmentId = (CollectionBookingId)message.ShipmentId;

        return InvokeAggregate(
            shipmentId,
            state =>
                CollectionBookingAggregate.SetAsBookedWithCarrier(
                    state.ProcessCategory,
                    state,
                    message.BookingReference
                )
        );
    }

    private Task HandleCarrierIntegrationCarrierCollectionBookingFailed(
        CarrierIntegrationEvents.CarrierCollectionBookingFailed message
    )
    {
        var shipmentId = (CollectionBookingId)message.ShipmentId;

        return InvokeAggregate(
            shipmentId,
            state =>
                CollectionBookingAggregate.SetAsCarrierCollectionBookingFailed(
                    state.ProcessCategory,
                    state,
                    message.Failure
                )
        );
    }

    private async Task InvokeAggregate(
        CollectionBookingId shipmentId,
        Func<CollectionBookingState, IEnumerable<BaseCollectionBookingEvent>> action
    )
    {
        // FIXME: Workaround against concurrency exceptions
        // await Task.Delay(TimeSpan.FromMilliseconds(20));

        CollectionBookingState collectionBookingState = default;
        var newEvents = await collectionBookingRepository.Upsert(
            shipmentId.ToEventStreamId(),
            stateProjection,
            state =>
            {
                collectionBookingState = state;
                return action(state) ?? Enumerable.Empty<BaseCollectionBookingEvent>();
            }
        );

        var delegatedDecisionsList = newEvents.Where(x => x.Delegated);
        foreach (var decision in delegatedDecisionsList)
        {
            await collectionBookingDelegator.DelegateDecision(collectionBookingState, decision);
        }
    }
}

public class TriggerNotSupportedException(string triggerType)
    : Exception($"Trigger '{triggerType}' is not supported");
