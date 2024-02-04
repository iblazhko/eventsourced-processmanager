namespace EventSourcedPM.Messaging.CollectionBooking.Commands;

using EventSourcedPM.Messaging.Models;

public class CreateCollectionBooking : BaseCollectionBookingCommand
{
    public ManifestedShipmentLeg CollectionLeg { get; init; }
    public string CollectionDate { get; init; }
    public string TimeZone { get; init; }
}

public class ScheduleCollectionBooking : BaseCollectionBookingCommand
{
    public string CollectionDate { get; init; }
}

public class BookCollectionWithCarrier : BaseCollectionBookingCommand;

public class CancelCollectionBooking : BaseCollectionBookingCommand;
