using ConsumerClient;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ApiClient>();
builder.Services.Configure<ApiClientConfig>(
    builder.Configuration.GetSection(nameof(ApiClientConfig)));

builder.Services.AddHttpClient<ApiClient>((serviceProvider, httpClient) => {
    var externalApiSettings = serviceProvider.GetRequiredService<IOptions<ApiClientConfig>>();
    httpClient.BaseAddress = new Uri(externalApiSettings.Value.BaseUri); 
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

record ApiResponse(DateOnly Date, int TemperatureC, string? Summary);

class ApiClientConfig
{
    public string BaseUri { get; set; }
}