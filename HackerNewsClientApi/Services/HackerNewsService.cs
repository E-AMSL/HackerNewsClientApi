using HackerNewsClient.Api.Controllers;
using HackerNewsClient.Api.Exceptions;
using HackerNewsClient.Api.Models;
using System.Text.Json;

namespace HackerNewsClient.Api.Services;

public class HackerNewsService : IHackerNewsService
{
    private readonly string bestStoriesAddress;
    private readonly string storyIdUrl;
    private readonly ILogger<HackerNewsController> logger;
    private readonly HttpClient httpClient;
    private readonly CachedItem<List<HackerNewsStory>> cachedStories;
    private readonly CachedItem<List<int>> cachedStoryIds;

    public HackerNewsService(ILogger<HackerNewsController> logger, HttpClient httpClient, IConfiguration configuration)
    {
        this.logger = logger;
        this.httpClient = httpClient;

        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
        bestStoriesAddress = appSettings?.BestStoriesAddress ?? "https://hacker-news.firebaseio.com/v0/beststories.json";
        storyIdUrl = appSettings?.StoryAddress ?? "https://hacker-news.firebaseio.com/v0/item/";

        cachedStories = new();
        cachedStoryIds = new();
    }

    public async Task<ServiceResponse<IAsyncEnumerable<HackerNewsStory>>> GetHackerNewsStoriesAsync(int amount)
    {
        logger.LogTrace($"{nameof(HackerNewsService)}.{nameof(GetHackerNewsStoriesAsync)}");

        ServiceResponse<IAsyncEnumerable<HackerNewsStory>> result = new();

        try
        {
            IEnumerable<int> storyIds;
            if (cachedStoryIds.IsCacheValid())
            {
                storyIds = cachedStoryIds.Item;
            }
            else
            {
                using HttpResponseMessage hackerResponse = await httpClient.GetAsync(bestStoriesAddress);
                hackerResponse.EnsureSuccessStatusCode();
                string responseBody = await hackerResponse.Content.ReadAsStringAsync();
                storyIds = JsonSerializer.Deserialize<IEnumerable<int>>(responseBody) ?? throw new JsonParsingException();
            }

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

        IEnumerable<HackerNewsStory>? storiesFromCache = cachedStories.Item?.Where(x => storyIds.Contains(x.Id));
        IEnumerable<int> storiesToGet = storyIds;

        if (storiesFromCache != null)
        {
            storiesToGet = storyIds.Except(storiesFromCache.Select(x => x.Id));

            foreach (var story in storiesFromCache)
            {
                yield return story;
            }
        }

        foreach (var id in storiesToGet)
        {
            if (cachedStories.IsCacheValid() && cachedStories.Item.Any(x => x.Id == id))
            {
                yield return cachedStories.Item.First(x => x.Id == id);
            }

            using HttpResponseMessage hackerResponse = await httpClient.GetAsync($"{storyIdUrl}/{id}.json");
            hackerResponse.EnsureSuccessStatusCode();
            string responseBody = await hackerResponse.Content.ReadAsStringAsync();

            HackerNewsStoryDto hackerNewsStory = JsonSerializer.Deserialize<HackerNewsStoryDto>(responseBody) 
                ?? throw new JsonParsingException($"Could not parse story :{id}");

            yield return hackerNewsStory.ToHackerNewsStory();
        }
    }
}
