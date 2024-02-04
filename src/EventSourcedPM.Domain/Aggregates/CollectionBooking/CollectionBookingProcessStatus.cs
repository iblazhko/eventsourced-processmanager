namespace EventSourcedPM.Domain.Aggregates.CollectionBooking;

using System;
using OneOf;

public class CollectionBookingProcessStatus
    : OneOfBase<
        CollectionBookingProcessStatus.InitializedStatus,
        CollectionBookingProcessStatus.ScheduledStatus,
        CollectionBookingProcessStatus.SchedulingFailedStatus,
        CollectionBookingProcessStatus.BookingStartedStatus,
        CollectionBookingProcessStatus.BookedStatus,
        CollectionBookingProcessStatus.BookingFailedStatus,
        CollectionBookingProcessStatus.BookingCancellationStartedStatus,
        CollectionBookingProcessStatus.BookingCancelledStatus,
        CollectionBookingProcessStatus.BookingCancellationFailedStatus
    >
{
    // ReSharper disable NotAccessedPositionalProperty.Global

    public record InitializedStatus;

    public record ScheduledStatus(DateTime BookAt);

    public record SchedulingFailedStatus(string Failure);

    public record BookingStartedStatus;

    public record BookedStatus(string BookingReference);

    public record BookingFailedStatus(string Failure);

    public record BookingCancellationStartedStatus(string BookingReference);

    public record BookingCancelledStatus;

    public record BookingCancellationFailedStatus(string Failure);

    public static CollectionBookingProcessStatus Initialized() => new(new InitializedStatus());

    public static CollectionBookingProcessStatus Scheduled(DateTime bookAt) =>
        new(new ScheduledStatus(bookAt));

    public static CollectionBookingProcessStatus SchedulingFailed(string failure) =>
        new(new SchedulingFailedStatus(failure));

    public static CollectionBookingProcessStatus BookingStarted() =>
        new(new BookingStartedStatus());

    public static CollectionBookingProcessStatus Booked(string bookingReference) =>
        new(new BookedStatus(bookingReference));

    public static CollectionBookingProcessStatus BookingFailed(string failure) =>
        new(new BookingFailedStatus(failure));

    public static CollectionBookingProcessStatus CancellationStarted(string bookingReference) =>
        new(new BookingCancellationStartedStatus(bookingReference));

    public static CollectionBookingProcessStatus Cancelled() => new(new BookingCancelledStatus());

    public static CollectionBookingProcessStatus CancellationFailed(string failure) =>
        new(new BookingCancellationFailedStatus(failure));

    private CollectionBookingProcessStatus(
        OneOf<
            InitializedStatus,
            ScheduledStatus,
            SchedulingFailedStatus,
            BookingStartedStatus,
            BookedStatus,
            BookingFailedStatus,
            BookingCancellationStartedStatus,
            BookingCancelledStatus,
            BookingCancellationFailedStatus
        > input
    )
        : base(input) { }
}
