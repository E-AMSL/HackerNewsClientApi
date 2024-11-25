namespace HackerNewsClient.Api.Services;

public interface IHackerNewsService
{
    Task<ServiceResponseAsync<IEnumerable<HnNews>>> GetHnNews();
}