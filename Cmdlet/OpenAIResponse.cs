namespace OpenAICmdlet;
public interface IOpenAIResponse
{
    public string Prompt { get; set; }
    public string Response { get; set; }
    public string[] History { get; set; }
}
