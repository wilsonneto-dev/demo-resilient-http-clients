using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ConsumerClient;

class ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
{
    public async Task<IEnumerable<ApiResponse>> GetWeather()
    {
        var timer = new Stopwatch();
        timer.Start();

        var response = await httpClient.GetAsync("weather");
        var json = await response.Content.ReadAsStringAsync();
        
        timer.Stop();
        logger.LogInformation("API request: {Status} - {Time}ms", response.StatusCode, timer.ElapsedMilliseconds);
        
        if (!response.IsSuccessStatusCode)
        {
            var problem = JsonSerializer.Deserialize<ProblemDetails>(json);
            throw new Exception(problem?.Detail ?? "not serialized");
        }
        
        var weather = JsonSerializer.Deserialize<IEnumerable<ApiResponse>>(json, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });
        
        return weather!;
    }
}