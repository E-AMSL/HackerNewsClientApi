using HackerNewsClient.Api.Models;

namespace HackerNewsClient.Api.Services;

public interface IHackerNewsService
{
    Task<ServiceResponse<IAsyncEnumerable<HackerNewsStory>>> GetHackerNewsStoriesAsync(int amount);
}