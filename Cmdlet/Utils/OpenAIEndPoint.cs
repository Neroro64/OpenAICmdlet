namespace OpenAICmdlet;
internal static class OpenAIEndpoint
{
    internal enum OpenAITask
    {
        TextCompletion, ChatCompletion,
        ImageGeneration, ImageEdit, ImageVariation,
        AudioTranscription, AudioTranslation
    }
    internal static readonly Uri Default = Get(OpenAITask.ChatCompletion);
    internal static readonly string Root = "https://api.openai.com/v1";
    internal static readonly Dictionary<OpenAITask, string> TaskEndpoint = new(){
        {OpenAITask.TextCompletion, "/completions"},
        {OpenAITask.ChatCompletion, "/chat/completions"},
        {OpenAITask.ImageGeneration, "/image/generations"},
        {OpenAITask.ImageEdit, "/images/edits"},
        {OpenAITask.ImageVariation, "/images/variations"},
        {OpenAITask.AudioTranscription, "/audio/transcriptions"},
        {OpenAITask.AudioTranslation, "/audio/translations"},
    };
    internal static Uri Get(OpenAITask task) => new Uri(Root + TaskEndpoint[task]);
}