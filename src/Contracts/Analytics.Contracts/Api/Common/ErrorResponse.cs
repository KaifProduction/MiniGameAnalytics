namespace Analytics.Contracts.Api.Common;

public sealed record ErrorResponse(
    string Code, 
    string Message, 
    IDictionary<string, string>? Details = null);