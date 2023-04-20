namespace OpenAICmdlet;
internal static class OpenAIEndpoint
{
    internal enum OpenAITask
    {
        TextCompletion, ChatCompletion,
        ImageGeneration, ImageEdit, ImageVariation,
        AudioTranscription, AudioTranslation
    }
    internal const string Root = "https://api.openai.com/v1";
    internal static string TaskEndpoint(OpenAITask task) => task switch
    {
        OpenAITask.TextCompletion => "/completions",
        OpenAITask.ChatCompletion => "/chat/completions",
        OpenAITask.ImageGeneration => "/image/generations",
        OpenAITask.ImageEdit => "/images/edits",
        OpenAITask.ImageVariation => "/images/variations",
        OpenAITask.AudioTranscription => "/audio/transcriptions",
        OpenAITask.AudioTranslation => "/audio/translations",
        _ => throw new ArgumentException("Invalid OpenAITask provided.")
    };

    internal static readonly Uri Default = Get(OpenAITask.ChatCompletion);
    internal static Uri Get(OpenAITask task) => new Uri(Root + TaskEndpoint(task));
}