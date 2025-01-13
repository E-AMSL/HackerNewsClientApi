namespace HackerNewsClient.Api.Models;

internal class CachedItem<T>
{
    public T Item { get; set; }
    public DateTimeOffset CachedTime { get; set; }
}