namespace Analytics.Contracts.Messaging.V1;

public sealed record EventMessage(
    Guid EventId,
    string GameId,
    string UserId,
    string SessionId,
    string EventType,
    long TimestampUtcUnixMs,
    IReadOnlyDictionary<string, string>? Props);