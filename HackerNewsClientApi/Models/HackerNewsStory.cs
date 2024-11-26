using System.Text.Json.Serialization;

namespace HackerNewsClient.Api.Models;

public class HackerNewsStory
{
    [JsonIgnore]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Uri { get; set; }
    public string PostedBy { get; set; }
    public DateTimeOffset Time { get; set; }
    public int Score { get; set; }
    public int CommentCount { get; set; }
}