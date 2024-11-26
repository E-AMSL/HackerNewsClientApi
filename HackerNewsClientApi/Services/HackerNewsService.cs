using HackerNewsClient.Api.Controllers;
using HackerNewsClient.Api.Exceptions;
using HackerNewsClient.Api.Models;
using Polly;
using Polly.Retry;
using System.Text.Json;

namespace HackerNewsClient.Api.Services;

public class HackerNewsService : IHackerNewsService
{
    private readonly string bestStoriesAddress;
    private readonly string storyItemUrl;

    private readonly ILogger<HackerNewsController> logger;
    private readonly HttpClient httpClient;

    private readonly CachedItem<List<HackerNewsStory>> cachedStories;
    private readonly CachedItem<List<int>> cachedStoryIds;

    private readonly AsyncRetryPolicy retryPolicy;

    public HackerNewsService(ILogger<HackerNewsController> logger, HttpClient httpClient, IConfiguration configuration)
    {
        this.logger = logger;
        this.httpClient = httpClient;

        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
        bestStoriesAddress = appSettings?.BestStoriesAddress ?? "https://hacker-news.firebaseio.com/v0/beststories.json";
        storyItemUrl = appSettings?.StoryAddress ?? "https://hacker-news.firebaseio.com/v0/item/";

        cachedStories = new();
        cachedStoryIds = new();

        retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(retryCount: 5, x => TimeSpan.FromSeconds(x));
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
                PolicyResult<IEnumerable<int>> pollyResult = await retryPolicy.ExecuteAndCaptureAsync(async () =>
                {
                    using HttpResponseMessage hackerResponse = await httpClient.GetAsync(bestStoriesAddress);
                    hackerResponse.EnsureSuccessStatusCode();
                    string responseBody = await hackerResponse.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<IEnumerable<int>>(responseBody) ?? throw new JsonParsingException();
                });

                storyIds = pollyResult.Result;
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

        List<Task<PolicyResult<HttpResponseMessage>>> httpClientTasks = [];

        foreach (var id in storiesToGet)
        {

            Task<PolicyResult<HttpResponseMessage>> pollyResult = retryPolicy.ExecuteAndCaptureAsync(async () =>
            {
                return await httpClient.GetAsync($"{storyItemUrl}/{id}.json");
            });

            httpClientTasks.Add(pollyResult);
        }

        var responses = await Task.WhenAll(httpClientTasks);

        foreach (var pollyResponse in responses)
        {
            HttpResponseMessage httpResponse = pollyResponse.Result;
            httpResponse.EnsureSuccessStatusCode();
            string responseBody = await httpResponse.Content.ReadAsStringAsync();

            httpResponse.Dispose();

            HackerNewsStoryDto? hackerNewsStory = JsonSerializer.Deserialize<HackerNewsStoryDto>(responseBody);
            if (hackerNewsStory != null)
            {
                yield return hackerNewsStory.ToHackerNewsStory();
            }
        }
    }
}
