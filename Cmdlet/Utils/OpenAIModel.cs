namespace OpenAICmdlet;
internal static class OpenAIModel
{
    internal static string? TaskModel(OpenAITask task) => task switch
    {
        OpenAITask.TextCompletion => "text-davinci-003",
        OpenAITask.ChatCompletion => "gpt-3.5-turbo",
        OpenAITask.AudioTranscription => "whisper-1",
        OpenAITask.AudioTranslation => "whisper-1",
        OpenAITask.ImageGeneration => default,
        OpenAITask.ImageEdit => default,
        OpenAITask.ImageVariation => default,
        _ => throw new ArgumentException("Invalid OpenAITask provided.")
    };

    internal static readonly string? Default = TaskModel(OpenAITask.ChatCompletion);
}
