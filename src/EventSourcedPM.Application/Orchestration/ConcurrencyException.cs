namespace EventSourcedPM.Application.Orchestration;

using System;
using System.Linq;

public class ConcurrencyException(string shipmentId, string reason = null, Exception innerException = null)
    : Exception(
        string.Join(
            ", ",
            new[] { $"Concurrency exception while processing shipment {shipmentId}", reason ?? string.Empty }.Where(x => !string.IsNullOrEmpty(x))
        ),
        innerException
    );
