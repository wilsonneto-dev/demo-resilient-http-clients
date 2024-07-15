
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (ILogger<Program> logger) =>
    {
        int odd = Random.Shared.Next(0, 100);
        const int threshold = 33;
        if (odd < threshold /*DateTime.Now.Minute % 2 == 1*/)
        {
            logger.LogError("{Sec} - Returning Error", DateTime.Now.Second);
            return Results.Problem(new ProblemDetails()
            {
                Detail = $"Injected error",
                Title = "Injected error",
                Type = "InjectedError"
            });
        }
        
        const int thresholdDelay = 66;
        if (odd < thresholdDelay)
        {
            logger.LogWarning("Delayed request");
            await Task.Delay(10_000);
        }
        
        logger.LogInformation("Returning ok");
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return Results.Ok(forecast);
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}