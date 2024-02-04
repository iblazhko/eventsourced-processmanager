namespace EventSourcedPM.Messaging.CollectionBooking.Events;

using System;
using EventSourcedPM.Messaging.Models;

public class CollectionBookingInitialized : BaseCollectionBookingEvent
{
    public ManifestedShipmentLeg CollectionLeg { get; init; }
    public string CollectionDate { get; init; }
    public string TimeZone { get; init; }
}

public class CollectionBookingSchedulingFailed : BaseCollectionBookingEvent
{
    public Guid CarrierId { get; init; }
    public string CollectionDate { get; init; }
    public string Failure { get; init; }
}

public class CollectionBookingScheduled : BaseCollectionBookingEvent
{
    public Guid CarrierId { get; init; }
    public string CollectionDate { get; init; }
    public string BookAt { get; init; }
}

public class CollectionBookingWithCarrierStarted : BaseCollectionBookingEvent;

public class CollectionBooked : BaseCollectionBookingEvent
{
    public Guid CarrierId { get; init; }
    public string BookingReference { get; init; }
}

public class CollectionBookingFailed : BaseCollectionBookingEvent
{
    public string Failure { get; init; }
}

public class CollectionBookingCancellationStarted : BaseCollectionBookingEvent
{
    public string BookingReference { get; init; }
}

public class CollectionBookingCancelled : BaseCollectionBookingEvent
{
    public string BookingReference { get; init; }
}

public class CollectionBookingCancellationFailed : BaseCollectionBookingEvent
{
    public string Failure { get; init; }
}
