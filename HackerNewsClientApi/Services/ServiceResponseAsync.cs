namespace HackerNewsClient.Api.Services;

public class ServiceResponseAsync<T>
{
    public T? Result { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}