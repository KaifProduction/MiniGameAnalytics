using Asp.Versioning;
using Asp.Versioning.Builder;
using Analytics.Contracts.Api.Events.V1;
using Analytics.Ingest.Api.Application.Validation;
using Analytics.Ingest.Api.Grpc;
using Analytics.Ingest.Api.Infrastructure.Validation;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Serilog;

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();
Log.Logger = logger;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<GameEventDtoValidator>();

builder.Services.AddApiVersioning(o =>
{
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
    o.DefaultApiVersion = new ApiVersion(1, 0);
}).AddApiExplorer(o =>
{
    o.GroupNameFormat = "'v'VVV";
    o.SubstituteApiVersionInUrl = true;
});

builder.Services.AddGrpc();
builder.Services.AddHealthChecks();
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ingest v1"));
}

app.MapHealthChecks("/health");

// ===== REST: POST /v1/events =====
ApiVersionSet versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1,0))
    .ReportApiVersions()
    .Build();

var v1 = app.MapGroup("/v1").WithApiVersionSet(versionSet).HasApiVersion(1,0);

v1.MapPost("/events", async (GameEventDto dto, IPublishEndpoint bus) =>
    {
        Log.Information("REST: Publishing {EventType} for game {GameId}", dto.EventType, dto.GameId);

        await bus.Publish(dto);

        return Results.Accepted(value: new { accepted = true });
    })
    .AddEndpointFilter(new ValidationFilter<GameEventDto>()) 
    .WithName("PostEvent")
    .WithOpenApi();
app.MapGrpcService<IngestGrpcService>();

app.Run();

public partial class Program {}