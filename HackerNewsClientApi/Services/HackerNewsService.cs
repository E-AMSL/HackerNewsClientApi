using HackerNewsClient.Api.Controllers;
using HackerNewsClient.Api.Exceptions;
using HackerNewsClient.Api.Models;
using System.Text.Json;

namespace HackerNewsClient.Api.Services;

public class HackerNewsService : IHackerNewsService
{
    private readonly string popularStoryIdsUrl = "https://hacker-news.firebaseio.com/v0/beststories.json";
    private readonly string storyIdUrl = "https://hacker-news.firebaseio.com/v0/item/";
    private readonly ILogger<HackerNewsController> logger;
    private readonly HttpClient httpClient;

    private readonly TimeSpan cacheLifetime = TimeSpan.FromMinutes(5);
    private readonly CachedItem<List<HackerNewsStory>> cachedStories;

    public HackerNewsService(ILogger<HackerNewsController> logger, HttpClient httpClient)
    {
        this.logger = logger;
        this.httpClient = httpClient;
        cachedStories = new();
    }

    public async Task<ServiceResponse<IAsyncEnumerable<HackerNewsStory>>> GetHackerNewsStoriesAsync(int amount)
    {
        logger.LogTrace($"{nameof(HackerNewsService)}.{nameof(GetHackerNewsStoriesAsync)}");

        ServiceResponse<IAsyncEnumerable<HackerNewsStory>> result = new();

        if (await IsStoryCacheValid(amount))
        {
            result.Response = cachedStories.Item.Take(amount).ToAsyncEnumerable();
            result.Success = true;
            return result;
        }

        try
        {
            using HttpResponseMessage hackerResponse = await httpClient.GetAsync(popularStoryIdsUrl);
            hackerResponse.EnsureSuccessStatusCode();
            string responseBody = await hackerResponse.Content.ReadAsStringAsync();
            var storyIds = JsonSerializer.Deserialize<IEnumerable<int>>(responseBody) ?? throw new JsonParsingException();

            IAsyncEnumerable<HackerNewsStory> stories = GetStoriesFromIdsAsync(storyIds.Take(amount));

            cachedStories.Item = await stories.ToListAsync(); //To avoid deffered execution during cache validation which can result in an endless loop
            cachedStories.CachedTime = DateTimeOffset.UtcNow;

            result.Success = true;
            result.Response = stories;
        }
        catch (HttpRequestException ex)
        {
            result.Success = false;
            result.Message = "HackerNews API responded with error code";
            logger.LogError(ex, result.Message);
        }
        catch (JsonParsingException ex)
        {
            result.Success = false;
            result.Message = "Could not parse response from HackerNews API";
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

    private async IAsyncEnumerable<HackerNewsStory> GetStoriesFromIdsAsync(IEnumerable<int> storyIds)
    {
        logger.LogTrace($"{nameof(HackerNewsService)}.{nameof(GetStoriesFromIdsAsync)}");

        foreach (var id in storyIds)
        {
            using HttpResponseMessage hackerResponse = await httpClient.GetAsync($"{storyIdUrl}/{id}.json");
            hackerResponse.EnsureSuccessStatusCode();
            string responseBody = await hackerResponse.Content.ReadAsStringAsync();

            HackerNewsStoryDto hackerNewsStory = JsonSerializer.Deserialize<HackerNewsStoryDto>(responseBody) 
                ?? throw new JsonParsingException($"Could not parse story :{id}");

            yield return hackerNewsStory.ToHackerNewsStory();
        }
    }

    private async Task<bool> IsStoryCacheValid(int amount)
    {
        logger.LogTrace($"{nameof(HackerNewsService)}.{nameof(IsStoryCacheValid)}");

        return cachedStories.Item is not null
               && cachedStories.Item.Count() >= amount
               && cachedStories.CachedTime + cacheLifetime > DateTimeOffset.UtcNow;
    }
}
