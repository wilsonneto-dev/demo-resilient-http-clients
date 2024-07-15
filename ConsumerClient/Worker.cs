namespace ConsumerClient;

internal class Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory) 
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var client = scope.ServiceProvider.GetRequiredService<ApiClient>();
                var weather = await client.GetWeather();
            }
            catch (Exception exception)
            {
                logger.LogError("[Worker] Client issue: {Error}", exception.Message);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}