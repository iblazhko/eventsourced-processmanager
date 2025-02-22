namespace EventSourcedPM.Domain.Aggregates.Orchestration;

using OneOf;

public class ShipmentProcessStageStatus
    : OneOfBase<
        ShipmentProcessStageStatus.NotRequiredStatus,
        ShipmentProcessStageStatus.NotStartedStatus,
        ShipmentProcessStageStatus.StartedStatus,
        ShipmentProcessStageStatus.RestartedStatus,
        ShipmentProcessStageStatus.CompletedStatus,
        ShipmentProcessStageStatus.FailedStatus
    >
{
    public record NotRequiredStatus;

    public record NotStartedStatus;

    public record StartedStatus;

    public record RestartedStatus;

    public record CompletedStatus;

    public record FailedStatus(string Failure);

    public static ShipmentProcessStageStatus NotRequired() => new(new NotRequiredStatus());

    public static ShipmentProcessStageStatus NotStarted() => new(new NotStartedStatus());

    public static ShipmentProcessStageStatus Started() => new(new StartedStatus());

    public static ShipmentProcessStageStatus Restarted() => new(new RestartedStatus());

    public static ShipmentProcessStageStatus Completed() => new(new CompletedStatus());

    public static ShipmentProcessStageStatus Failed(string failure) => new(new FailedStatus(failure));

    private ShipmentProcessStageStatus(
        OneOf<NotRequiredStatus, NotStartedStatus, StartedStatus, RestartedStatus, CompletedStatus, FailedStatus> input
    )
        : base(input) { }

    public bool IsCompleted() => IsT4;

    public bool IsCompletedOrNotRequired() => IsT4 || IsT5;
}
