namespace OpenAI;

public static class ApiCostEstimator
{
    private const float tokenizeRatio = 0.7f;
    private static readonly Dictionary<string, float> _CostTable = new()
    {
        ["text-davinci-003"] = 0.02f / 1000f, // per token
        ["gpt-3.5-turbo"] = 0.002f / 1000f,   // per token
        ["gpt-3.5-turbo-0301"] = 0.002f / 1000f,   // per token
        ["whisper-1"] = 0.006f,               // per minute
        ["256x256"] = 0.016f,                 // per image
        ["512x512"] = 0.018f,                 // per image
        ["1024x1024"] = 0.02f,                // per image
    };
    public static float EstimateTokenCost(string? inputPrompt, string? model, int samples)
    {
        if (inputPrompt == null || model == null)
            return 0;
        return inputPrompt.Split(" ").Length * tokenizeRatio * _CostTable[model] * samples;
    }

    public static float EstimateImageCost(string? size, string? inputPrompt, int samples)
    {
        if (size == null || !_CostTable.ContainsKey(size))
            return 0;

        inputPrompt ??= "";
        return EstimateTokenCost(inputPrompt, "text-davinci-003", 1) + _CostTable[size] * samples;
    }

    public static float EstimateAudioCost(float? audioMin, string? inputPrompt, int samples)
    {
        if (audioMin == null)
            return 0;
        inputPrompt ??= "";
        return EstimateTokenCost(inputPrompt, "text-davinci-003", 1) +
               _CostTable["whisper-1"] * audioMin.Value * samples;
    }
}
