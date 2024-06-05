using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddCheck<LiveHealthCheck>("live", tags: [ "live" ])
    .AddCheck<ReadyHealthCheck>("ready", tags: [ "ready" ])
    .AddCheck<StartupHealthCheck>("startup", tags: [ "startup" ]);

var app = builder.Build();

app.MapHealthChecks("health/live", new HealthCheckOptions() { Predicate = x => x.Tags.Contains("live") });
app.UseHealthChecks("/health/ready", new HealthCheckOptions() { Predicate = x => x.Tags.Contains("ready") });
app.UseHealthChecks("/health/startup", new HealthCheckOptions() { Predicate = x => x.Tags.Contains("startup") });

app.UseSwagger();
app.UseSwaggerUI();

int i = 0;

app.MapGet("/",  (IConfiguration config) => TypedResults.Ok(new EchoOutput(
        config.GetValue<string>("Version") ?? "No version...",
        Environment.MachineName,
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown",
        Interlocked.Increment(ref i),
        new(HealthState.Ready, HealthState.Startup, HealthState.Live)
    )))
    .WithOpenApi()
    .WithName("Echo");


app.MapPost("/health", (HealthStateDto newHealthState) => 
{
    HealthState.Ready = newHealthState.Ready;
    HealthState.Startup = newHealthState.Startup;
    HealthState.Live = newHealthState.Live;
    return Results.Ok(newHealthState);
}).WithName("Edit Health State");

app.Run();

record HealthStateDto(bool Ready, bool Startup, bool Live);
record EchoOutput(string Version, string HostName, string AssemblyVersion, int Counter, HealthStateDto Health);

public static class HealthState
{
    public static bool Ready { get; set; } = true;
    public static bool Startup { get; set; } = true;
    public static bool Live { get; set; } = true;
}

public class ReadyHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
        => Task.FromResult(HealthState.Ready ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());
}


public class StartupHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
        => Task.FromResult(HealthState.Startup ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());
}

public class LiveHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
        => Task.FromResult(HealthState.Live ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());
}