namespace Analytics.Contracts.Api.Events.V1;

public sealed record GameEventDto(
    Guid EventId,
    string GameId,
    string UserId,
    string SessionId,
    string EventType,
    DateTimeOffset TimestampUtc,
    IReadOnlyDictionary<string, object>? Props);