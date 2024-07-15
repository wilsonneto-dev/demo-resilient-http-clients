using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer().AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<WeatherForecastClient>();
builder.Services.Configure<WeatherForecastClientConfig>(
    builder.Configuration.GetSection(nameof(WeatherForecastClientConfig)));

builder.Services.AddHttpClient<WeatherForecastClient>((serviceProvider, httpClient) =>
{
    var externalApiSettings = serviceProvider.GetRequiredService<IOptions<WeatherForecastClientConfig>>();
    httpClient.BaseAddress = new Uri(externalApiSettings.Value.BaseUri);
})
    .AddResilienceHandler("WeatherForecastClientResilience", pipelineBuilder =>
{
    pipelineBuilder.AddHedging(new HttpHedgingStrategyOptions
    {
        Delay = TimeSpan.FromMilliseconds(300),
        MaxHedgedAttempts = 5
    });

    pipelineBuilder.AddTimeout(new HttpTimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(8)
    });

    pipelineBuilder.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 5,
        Delay = TimeSpan.FromMilliseconds(200),
        BackoffType = DelayBackoffType.Exponential,
        MaxDelay = TimeSpan.FromSeconds(5),
        UseJitter = true
    });

    pipelineBuilder.AddTimeout(new HttpTimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(1)
    });

    pipelineBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        BreakDuration = TimeSpan.FromSeconds(20),
        FailureRatio = 0.9,
        MinimumThroughput = 3,
        SamplingDuration = TimeSpan.FromSeconds(10)
    });

    pipelineBuilder.AddRateLimiter(new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
    {
        Window = TimeSpan.FromSeconds(10),
        PermitLimit = 4,
        QueueLimit = 0
    }));

    pipelineBuilder.AddConcurrencyLimiter(new ConcurrencyLimiterOptions
    {
        PermitLimit = 2
    }); 
});

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();

app.MapGet("/weather", async (WeatherForecastClient weatherForecastClient, ILogger<Program> logger) =>
{
    try
    {
        var weather = await weatherForecastClient.GetWeather();
        logger.LogInformation("Returning Ok");
        return Results.Ok(weather);
    }
    catch (Exception exception)
    {
        logger.LogError("Error received: {Error}", exception.Message);
        return Results.Problem(
            detail: exception.Message,
            type: exception.GetType().FullName,
            title: exception.GetType().FullName);
    }
});

app.Run();

class WeatherForecastClient(HttpClient httpClient, ILogger<WeatherForecastClient> logger)
{
    public async Task<IEnumerable<WeatherForecastResponse>> GetWeather()
    {
        var response = await httpClient.GetAsync("weatherforecast");
        var json = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var weather = JsonSerializer.Deserialize<IEnumerable<WeatherForecastResponse>>(json, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            return weather!;
        }

        var error = JsonSerializer.Deserialize<ProblemDetails>(json);
        throw new Exception(error?.Detail ?? "General error");
    }
}

record WeatherForecastResponse(DateOnly Date, int TemperatureC, string? Summary);

class WeatherForecastClientConfig
{
    public string BaseUri { get; set; }
}