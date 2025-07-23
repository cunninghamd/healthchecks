using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHealthChecks()
    .AddCheck(
        "sample_health_check",
        () => HealthCheckResult.Healthy("Sample check is healthy."),
        tags: new[] { "sample" }
    )
    .AddUrlGroup(
        new Uri("http://www.nosleep.ca/"),
        name: "nosleep.ca",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "nosleep" }
    )
    .AddUrlGroup(
        new Uri("http://localhost/"),
        name: "localhost",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "localhost" }
    );

builder.Services.AddHealthChecksUI(options =>
{
        options.SetEvaluationTimeInSeconds(5); //time in seconds between check
        options.MaximumHistoryEntriesPerEndpoint(60); //maximum history of checks
        options.SetApiMaxActiveRequests(1); //api requests concurrency

        options.AddHealthCheckEndpoint("nosleep", "/health-nosleep");
        options.AddHealthCheckEndpoint("localhost", "/health-localhost");
})
.AddInMemoryStorage();

var app = builder.Build();

app.MapHealthChecks("/health-nosleep", new HealthCheckOptions()
{
    Predicate = r => r.Tags.Contains("nosleep"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});
app.MapHealthChecks("/health-localhost", new HealthCheckOptions()
{
    Predicate = r => r.Tags.Contains("localhost"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

app.MapHealthChecksUI(options => options.UIPath = "/health-ui");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
