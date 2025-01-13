using HackerNewsClient.Api.Controllers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HackerNewsClient.Api.Services;

public class HackerNewsService : IHackerNewsService
{
    private readonly string hackerNewsApiEndpoint = "";
    private readonly ILogger<HackerNewsController> logger;
    private readonly HttpClient httpClient;

    public HackerNewsService(ILogger<HackerNewsController> logger, HttpClient httpClient)
    {
        this.logger = logger;
        this.httpClient = httpClient;
    }

    public async Task<ServiceResponseAsync<IEnumerable<HnNews>>> GetHnNews()
    {
        logger.LogTrace($"{nameof(HackerNewsService)}.{nameof(GetHnNews)}");

        ServiceResponseAsync<IEnumerable<HnNews>> result = new();

        try
        {
            using HttpResponseMessage hackerResponse = await httpClient.GetAsync(hackerNewsApiEndpoint);
            hackerResponse.EnsureSuccessStatusCode();
            string responseBody = await hackerResponse.Content.ReadAsStringAsync();
            JsonSerializer.Deserialize<IEnumerable<HnNews>>(responseBody);
        }
        catch (HttpRequestException ex)
        {
            result.Success = false;
            result.Message = "HackerNews API responded with error code";
            logger.LogError(ex, result.Message);
        }
        catch (Exception ex) 
        {
            result.Success = false;
            result.Message = "Failed to access HackerNews API";
            logger.LogError(ex, result.Message);
        }

        return result;
    }
}
