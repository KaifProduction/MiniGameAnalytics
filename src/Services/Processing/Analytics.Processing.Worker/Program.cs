using Analytics.Contracts.Api.Events.V1;
using MassTransit;
using Serilog;

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();
Log.Logger = logger;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSerilog();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<GameEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();

public class GameEventConsumer : IConsumer<GameEventDto>
{
    public Task Consume(ConsumeContext<GameEventDto> context)
    {
        Log.Information("Processing event {EventType} for game {GameId}",
            context.Message.EventType,
            context.Message.GameId);
        // TODO: валидация, нормализация, сохранение в Storage
        return Task.CompletedTask;
    }
}