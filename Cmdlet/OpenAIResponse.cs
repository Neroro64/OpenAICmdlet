namespace OpenAICmdlet;
public class OpenAIResponse
{
    public string Prompt { get; set; } = "";
    public string[] Response { get; set; } = { };
    public DateTime TimeStamp { get; set; } = DateTime.Now;
}
