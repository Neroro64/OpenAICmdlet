namespace OpenAI;
[Serializable]
public class Response
{
    public string Prompt { get; set; } = String.Empty;
    public IEnumerable<string> Body { get; set; } = Array.Empty<string>();
    public DateTime TimeStamp { get; set; } = DateTime.Now;
}
