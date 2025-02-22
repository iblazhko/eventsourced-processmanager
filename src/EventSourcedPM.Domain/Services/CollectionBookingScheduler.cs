namespace EventSourcedPM.Domain.Services;

using System;
using EventSourcedPM.Domain.Models;
using OneOf;

public readonly record struct CollectionBookingTime(DateTime BookAt); // assuming UTC

public readonly record struct CollectionBookingSchedulingFailure(string Failure);

public interface ICollectionBookingScheduler
{
    OneOf<CollectionBookingTime, CollectionBookingSchedulingFailure> ScheduleCollectionBooking(
        ManifestedShipmentLeg collectionLeg,
        DateOnly collectionDate,
        TimeZoneId timeZone
    );
}

public class CollectionBookingScheduler : ICollectionBookingScheduler
{
    public OneOf<CollectionBookingTime, CollectionBookingSchedulingFailure> ScheduleCollectionBooking(
        ManifestedShipmentLeg collectionLeg,
        DateOnly collectionDate,
        TimeZoneId timeZone
    )
    {
        // this example solution ignores time zone aspect

        var now = DateTime.UtcNow;
        var currentDate = new DateOnly(now.Date.Year, now.Date.Month, now.Date.Day);

        if (collectionDate < currentDate)
            return new CollectionBookingSchedulingFailure("Collection date cannot be in the past");

        const int maxDaysInTheFuture = 7;
        if (collectionDate > currentDate.AddDays(maxDaysInTheFuture))
            return new CollectionBookingSchedulingFailure($"Collection date cannot be after {maxDaysInTheFuture} days in the future");

        if (collectionDate == currentDate)
            return new CollectionBookingTime(now);

        return new CollectionBookingTime(new DateTime(collectionDate.AddDays(-1), EndOfDayCollectionBookingTime));
    }

    private static readonly TimeOnly EndOfDayCollectionBookingTime = new(22, 30, 0);
}
