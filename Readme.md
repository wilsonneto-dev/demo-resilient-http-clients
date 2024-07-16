# Resilient HTTP Client Demo

```csharp

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

```


This demo uses:
- .Net 8

---

| [<img src="https://github.com/wilsonneto-dev.png" width="75px;"/>][1] |
| :-: |
|[Wilson Neto][1]|


[1]: https://github.com/wilsonneto-dev
