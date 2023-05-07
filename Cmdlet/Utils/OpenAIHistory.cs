namespace OpenAICmdlet;
public class OpenAIHistory
{
    public List<OpenAISession> Sessions { get; init; } = new List<OpenAISession>();
}

public class OpenAISession
{
    public List<OpenAIResponse> Session { get; init; } = new List<OpenAIResponse>();
}
