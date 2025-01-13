using System.Text.Json.Serialization;

namespace HackerNewsClient.Api.Models;

public class HackerNewsStoryDto
{
    [JsonPropertyName("by")]
    public string By { get; set; }

    [JsonPropertyName("descendants")]
    public int Descendants { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("kids")]
    public IEnumerable<int> Kids { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    public HackerNewsStory ToHackerNewsStory()
    {
        return new HackerNewsStory
        {
            Id = Id,
            CommentCount = Kids.Count(),
            PostedBy = By,
            Score = Score,
            Time = DateTimeOffset.FromUnixTimeMilliseconds(Time),
            Title = Title,
            Uri = Url
        };
    }
}
