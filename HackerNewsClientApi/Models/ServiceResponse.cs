namespace HackerNewsClient.Api.Models;

public class ServiceResponse<T>
{
    public T? Response { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}