using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

int i = 0;

app.MapGet("/",  (IConfiguration config) => TypedResults.Ok(new EchoOutput(
        config.GetValue<string>("Version") ?? "No version...",
        Environment.MachineName,
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown",
        Interlocked.Increment(ref i)
    )))
    .WithOpenApi()
    .WithName("Echo");

app.Run();

record EchoOutput(string Version, string HostName, string AssemblyVersion, int Counter);