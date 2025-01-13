using HackerNewsClient.Api.Services;
using Microsoft.Extensions.Logging;

namespace HackerNewsClient.Api.Models;

internal class CachedItem<T>
{
    private readonly TimeSpan cacheLifetime = TimeSpan.FromMinutes(5);

    public T Item { get; set; }
    public DateTimeOffset CachedTime { get; set; }
    public bool IsCacheValid()
    {
        return Item is not null 
            && CachedTime + cacheLifetime > DateTimeOffset.UtcNow;
    }
}