namespace EventSourcedPM.Domain.Aggregates.Orchestration;

using System;
using System.Linq;
using EventSourcedPM.Domain.Models;
using EventSourcedPM.Messaging.Orchestration.Events;
using EventSourcedPM.Ports.EventStore;

// ReSharper disable once UnusedType.Global
public class ShipmentProcessStateProjection
    : IEventStreamProjection<ShipmentProcessState, BaseShipmentProcessEvent>
{
    public ShipmentProcessState GetInitialState(string streamId) =>
        new(default, default, default, default, default, default);

    public ShipmentProcessState Apply(ShipmentProcessState state, BaseShipmentProcessEvent evt) =>
        evt switch
        {
            ShipmentProcessStarted x
                => new ShipmentProcessState(
                    (ShipmentProcessCategory)x.ProcessCategory,
                    (ShipmentProcessId)x.ShipmentId,
                    new ShipmentProcessInput(
                        x.Legs?.Select(l => l.ToDomain()).ToArray(),
                        DateOnly.TryParse(x.CollectionDate, out var d)
                            ? d
                            : DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
                        (TimeZoneId)x.TimeZone
                    ),
                    new ShipmentProcessOutcome(
                        Array.Empty<ManifestedShipmentLeg>(),
                        new ShipmentDocuments(default, default, default, default),
                        default
                    ),
                    ShipmentProcessStep.ShipmentProcessStarted,
                    new ShipmentProcessStagesProgress(
                        ShipmentProcessStageStatus.Started(),
                        ShipmentProcessStageStatus.NotStarted(),
                        ShipmentProcessStageStatus.NotStarted(),
                        ShipmentProcessStageStatus.NotStarted(),
                        ShipmentProcessStageStatus.NotStarted(),
                        ShipmentProcessStageStatus.NotStarted(),
                        ShipmentProcessStageStatus.NotStarted(),
                        ShipmentProcessStageStatus.NotStarted()
                    )
                ),
            ShipmentProcessCompleted
                => state with
                {
                    ProcessStep = ShipmentProcessStep.ShipmentProcessCompleted,
                    StagesProgress = state.StagesProgress with
                    {
                        OverallProcess = ShipmentProcessStageStatus.Completed()
                    }
                },
            ShipmentProcessFailed x
                => state with
                {
                    ProcessStep = ShipmentProcessStep.ShipmentProcessFailed,
                    StagesProgress = state.StagesProgress with
                    {
                        OverallProcess = ShipmentProcessStageStatus.Failed(x.Failure)
                    }
                },
            ManifestationAndDocumentsStarted
                => state with
                {
                    ProcessStep = ShipmentProcessStep.ManifestationAndDocumentsStarted,
                    StagesProgress = state.StagesProgress with
                    {
                        ManifestationAndDocuments = ShipmentProcessStageStatus.Started()
                    }
                },
            ManifestationAndDocumentsCompleted
                => state with
                {
                    ProcessStep = ShipmentProcessStep.ManifestationAndDocumentsCompleted,
                    StagesProgress = state.StagesProgress with
                    {
                        ManifestationAndDocuments = ShipmentProcessStageStatus.Completed()
                    }
                },
            ManifestationAndDocumentsFailed x
                => state with
                {
                    ProcessStep = ShipmentProcessStep.ManifestationAndDocumentsFailed,
                    StagesProgress = state.StagesProgress with
                    {
                        ManifestationAndDocuments = ShipmentProcessStageStatus.Failed(x.Failure)
                    }
                },
            CustomsInvoiceGenerationStarted
                => state with
                {
                    ProcessStep = ShipmentProcessStep.CustomsInvoiceGenerationStarted,
                    StagesProgress = state.StagesProgress with
                    {
                        CustomsInvoiceGeneration = ShipmentProcessStageStatus.Started()
                    }
                },
            CustomsInvoiceGenerationCompleted x
                => state with
                {
                    ProcessStep = ShipmentProcessStep.CustomsInvoiceGenerationCompleted,
                    StagesProgress = state.StagesProgress with
                    {
                        CustomsInvoiceGeneration = ShipmentProcessStageStatus.Completed()
                    },
                    ProcessOutcome = state.ProcessOutcome with
                    {
                        Documents = state.ProcessOutcome.Documents with
                        {
                            CustomsInvoice = (DocumentLocation)x.CustomsInvoice
                        }
                    }
                },
            CustomsInvoiceGenerationFailed x
                => state with
                {
                    ProcessStep = ShipmentProcessStep.CustomsInvoiceGenerationFailed,
                    StagesProgress = state.StagesProgress with
                    {
                        CustomsInvoiceGeneration = ShipmentProcessStageStatus.Failed(x.Failure)
                    },
                    ProcessOutcome = state.ProcessOutcome with
                    {
                        Documents = state.ProcessOutcome.Documents with { CustomsInvoice = default }
                    }
                },
            ShipmentManifestationStarted
                => state with
                {
                    StagesProgress = state.StagesProgress with
                    {
                        ShipmentManifestation = ShipmentProcessStageStatus.Started()
                    },
                    ProcessStep = ShipmentProcessStep.ShipmentManifestationStarted
                },
            ShipmentManifestationCompleted x
                => state with
                {
                    ProcessStep = ShipmentProcessStep.ShipmentManifestationCompleted,
                    StagesProgress = state.StagesProgress with
                    {
                        ShipmentManifestation = ShipmentProcessStageStatus.Completed()
                    },
                    ProcessOutcome = state.ProcessOutcome with
                    {
                        ManifestedLegs = x.ManifestedLegs.Select(l => l.ToDomain()).ToArray()
                    }
                },
            ShipmentManifestationFailed x
                => state with
                {
                    ProcessStep = ShipmentProcessStep.ShipmentManifestationFailed,
                    StagesProgress = state.StagesProgress with
                    {
                        ShipmentManifestation = ShipmentProcessStageStatus.Failed(x.Failure)
                    },
                    ProcessOutcome = state.ProcessOutcome with
                    {
                        ManifestedLegs = Array.Empty<ManifestedShipmentLeg>()
                    }
                },
            ShipmentLabelsGenerationStarted
                => state with
                {
                    ProcessStep = ShipmentProcessStep.ShipmentLabelsGenerationStarted,
                    StagesProgress = state.StagesProgress with
                    {
                        ShipmentLabels = ShipmentProcessStageStatus.Started()
                    }
                },
            ShipmentLabelsGenerationCompleted x
                => state with
                {
                    ProcessStep = ShipmentProcessStep.ShipmentLabelsGenerationCompleted,
                    StagesProgress = state.StagesProgress with
                    {
                        ShipmentLabels = ShipmentProcessStageStatus.Completed()
                    },
                    ProcessOutcome = state.ProcessOutcome with
                    {
                        Documents = state.ProcessOutcome.Documents with
                        {
                            Labels = (DocumentLocation)x.ShipmentLabels
                        }
                    }
                },
            ShipmentLabelsGenerationFailed x
                => state with
                {
                    ProcessStep = ShipmentProcessStep.ShipmentLabelsGenerationFailed,
                    StagesProgress = state.StagesProgress with
                    {
                        ShipmentLabels = ShipmentProcessStageStatus.Failed(x.Failure)
                    },
                    ProcessOutcome = state.ProcessOutcome with
                    {
                        Documents = state.ProcessOutcome.Documents with { Labels = default }
                    }
                },
            ReceiptGenerationStarted
                => state with
                {
                    ProcessStep = ShipmentProcessStep.ReceiptGenerationStarted,
                    StagesProgress = state.StagesProgress with
                    {
                        Receipt = ShipmentProcessStageStatus.Started()
                    }
                },
            ReceiptGenerationCompleted x
                => state with
                {
                    ProcessStep = ShipmentProcessStep.ReceiptGenerationCompleted,
                    StagesProgress = state.StagesProgress with
                    {
                        Receipt = ShipmentProcessStageStatus.Completed()
                    },
                    ProcessOutcome = state.ProcessOutcome with
                    {
                        Documents = state.ProcessOutcome.Documents with
                        {
                            Receipt = (DocumentLocation)x.Receipt
                        }
                    }
                },
            ReceiptGenerationFailed x
                => state with
                {
                    ProcessStep = ShipmentProcessStep.ReceiptGenerationFailed,
                    StagesProgress = state.StagesProgress with
                    {
                        Receipt = ShipmentProcessStageStatus.Failed(x.Failure)
                    },
                    ProcessOutcome = state.ProcessOutcome with
                    {
                        Documents = state.ProcessOutcome.Documents with { Receipt = default }
                    }
                },
            CombinedDocumentGenerationStarted
                => state with
                {
                    ProcessStep = ShipmentProcessStep.CombinedDocumentGenerationStarted,
                    StagesProgress = state.StagesProgress with
                    {
                        CombinedDocument = ShipmentProcessStageStatus.Started()
                    }
                },
            CombinedDocumentGenerationCompleted x
                => state with
                {
                    ProcessStep = ShipmentProcessStep.CombinedDocumentGenerationCompleted,
                    StagesProgress = state.StagesProgress with
                    {
                        CombinedDocument = ShipmentProcessStageStatus.Completed()
                    },
                    ProcessOutcome = state.ProcessOutcome with
                    {
                        Documents = state.ProcessOutcome.Documents with
                        {
                            CombinedDocument = (DocumentLocation)x.CombinedDocument
                        }
                    }
                },
            CombinedDocumentGenerationFailed x
                => state with
                {
                    ProcessStep = ShipmentProcessStep.CombinedDocumentGenerationFailed,
                    StagesProgress = state.StagesProgress with
                    {
                        CombinedDocument = ShipmentProcessStageStatus.Failed(x.Failure)
                    },
                    ProcessOutcome = state.ProcessOutcome with
                    {
                        Documents = state.ProcessOutcome.Documents with
                        {
                            CombinedDocument = default
                        }
                    }
                },
            CollectionBookingStarted
                => state with
                {
                    ProcessStep = ShipmentProcessStep.CollectionBookingStarted,
                    StagesProgress = state.StagesProgress with
                    {
                        CollectionBooking = ShipmentProcessStageStatus.Started()
                    }
                },
            CollectionBookingCompleted x
                => state with
                {
                    ProcessStep = ShipmentProcessStep.CollectionBookingCompleted,
                    StagesProgress = state.StagesProgress with
                    {
                        CollectionBooking = ShipmentProcessStageStatus.Completed()
                    },
                    ProcessOutcome = state.ProcessOutcome with
                    {
                        CollectionBookingReference = x.BookingReference
                    }
                },
            CollectionBookingFailed x
                => state with
                {
                    ProcessStep = ShipmentProcessStep.CollectionBookingFailed,
                    StagesProgress = state.StagesProgress with
                    {
                        CollectionBooking = ShipmentProcessStageStatus.Failed(x.Failure)
                    },
                    ProcessOutcome = state.ProcessOutcome with
                    {
                        CollectionBookingReference = default
                    }
                },
            _ => state
        };
}
