namespace OpenAICmdlet;
public class OpenAIResponse
{
    public string Prompt { get; set; } = String.Empty;
    public IEnumerable<string> Response { get; set; } = Array.Empty<string>();
    public DateTime TimeStamp { get; set; } = DateTime.Now;
}
