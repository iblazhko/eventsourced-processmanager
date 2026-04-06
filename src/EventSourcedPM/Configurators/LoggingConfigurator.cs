namespace EventSourcedPM.Configurators;

public static class LoggingConfigurator
{
    internal static LogLevel GetLevelValue(string level) =>
        (level?.ToUpperInvariant() ?? string.Empty) switch
        {
            "VRB" or "VERBOSE" or "TRACE" => LogLevel.Trace,
            "DBG" or "DEBUG" => LogLevel.Debug,
            "INF" or "INFO" or "INFORMATION" => LogLevel.Information,
            "WRN" or "WARN" or "WARNING" => LogLevel.Warning,
            "ERR" or "ERROR" => LogLevel.Error,
            "FATAL" or "CRITICAL" => LogLevel.Critical,
            _ => LogLevel.None
        };
}
