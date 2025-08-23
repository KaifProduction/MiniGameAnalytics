using Analytics.Contracts.Api.Events.V1;
using Analytics.Contracts.Grpc.Ingest.V1;
using FluentValidation;
using Grpc.Core;
using MassTransit;
using Serilog;

namespace Analytics.Ingest.Api.Grpc;

public sealed class IngestGrpcService : IngestService.IngestServiceBase
{
    private readonly IPublishEndpoint _bus;
    private readonly IValidator<GameEventDto> _validator;

    public IngestGrpcService(IPublishEndpoint bus, IValidator<GameEventDto> validator)
    {
        _bus = bus;
        _validator = validator;
    }

    public override async Task<Ack> SendEvent(GameEvent request, ServerCallContext context)
    {
        var dto = new GameEventDto(
            Guid.Parse(request.EventId),
            request.GameId,
            request.UserId,
            request.SessionId,
            request.EventType,
            DateTimeOffset.FromUnixTimeMilliseconds(request.TimestampUtcUnixMs).ToUniversalTime(),
            request.Props?.ToDictionary(kv => (string)kv.Key, kv => (object)kv.Value)
        );

        var result = await _validator.ValidateAsync(dto, context.CancellationToken);
        if (!result.IsValid)
        {
            var md = new Metadata();
            foreach (var e in result.Errors)
                md.Add($"validation-{e.PropertyName}", e.ErrorMessage);

            throw new RpcException(new Status(StatusCode.InvalidArgument, "Validation failed"), md);
        }

        Log.Information("gRPC: publishing {EventType} for game {GameId}", dto.EventType, dto.GameId);
        await _bus.Publish(dto, context.CancellationToken);

        return new Ack { Accepted = true };
    }
}