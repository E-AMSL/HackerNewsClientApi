namespace HackerNewsClientApi.Services;

public interface IHackerNewsService
{
    ServiceResult<IEnumerable<HnNews>> GetHnNews();
}