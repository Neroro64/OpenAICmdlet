namespace OpenAI;
public enum OpenAITask
{
    TextCompletion,
    ChatCompletion,
    Embeddings,
    ImageGeneration,
    ImageEdit,
    ImageVariation,
    AudioTranscription,
    AudioTranslation
}

[Flags]
public enum TaskCategory
{
    None = 0b000,
    Text = 0b001,
    Image = 0b010,
    Audio = 0b100,
    All = 0b111
}
