namespace HackerNewsClient.Api.Services;

public interface IHackerNewsService
{
    ServiceResult<IEnumerable<HnNews>> GetHnNews();
}