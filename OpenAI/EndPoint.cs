namespace OpenAI;
public static class Endpoint
{
    public const string Root = "https://api.openai.com/v1";
    public static string TaskEndpoint(OpenAITask task) => task switch
    {
        OpenAITask.TextCompletion => "/completions",
        OpenAITask.ChatCompletion => "/chat/completions",
        OpenAITask.ImageGeneration => "/images/generations",
        OpenAITask.ImageEdit => "/images/edits",
        OpenAITask.ImageVariation => "/images/variations",
        OpenAITask.AudioTranscription => "/audio/transcriptions",
        OpenAITask.AudioTranslation => "/audio/translations",
        _ => throw new ArgumentException("Invalid OpenAITask provided.")
    };

    public static readonly Uri Default = Get(OpenAITask.ChatCompletion);
    public static Uri Get(OpenAITask task) => new Uri(Root + TaskEndpoint(task));
}
