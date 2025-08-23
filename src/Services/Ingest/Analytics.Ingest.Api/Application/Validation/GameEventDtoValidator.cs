using Analytics.Contracts.Api.Events.V1;
using FluentValidation;

namespace Analytics.Ingest.Api.Application.Validation;

public sealed class GameEventDtoValidator : AbstractValidator<GameEventDto>
{
    public GameEventDtoValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.GameId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.UserId).NotEmpty().MaximumLength(128);
        RuleFor(x => x.SessionId).NotEmpty().MaximumLength(128);
        RuleFor(x => x.EventType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.TimestampUtc)
            .NotEmpty()
            .Must(t => t.Offset == TimeSpan.Zero).WithMessage("TimestampUtc must be UTC (offset +00:00)")
            .LessThanOrEqualTo(DateTimeOffset.UtcNow.AddMinutes(5));
      
        RuleFor(x => x.Props).Must(p => p == null || p.Count <= 64)
            .WithMessage("Props max 64 keys");
    }
}