namespace HackerNewsClient.Api.Exceptions;

public class JsonParsingException : Exception
{
    public JsonParsingException(string message) : base(message)
    {
    }

    public JsonParsingException()
    {
    }
}
