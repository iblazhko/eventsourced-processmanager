namespace EventSourcedPM.Application.Orchestration;

using Serilog;

internal static class DelegatorLogger
{
    public static void LogDelegatingMessage<TMessage, TDelegatedMessage>(TMessage message, TDelegatedMessage delegatedMessage)
    {
        // csharpier-ignore
        Log.Information(
            "Delegating {MessageType} -> {DelegatedMessageType}",
            typeof(TMessage).FullName,
            typeof(TDelegatedMessage).FullName);
    }

    public static void LogCannotelegateMessage<TMessage, TDelegatedMessage>(string reason)
    {
        Log.Warning(
            "Cannot delegate {MessageType} -> {DelegatedMessageType}: " + reason,
            typeof(TMessage).FullName,
            typeof(TDelegatedMessage).FullName
        );
    }
}
