namespace OpenAICmdlet;
internal static class Constant
{
    internal const int HTTP_TIMEOUT_MIN = 5;
    internal const int RW_LOCK_TIMEOUT_MS = 100;
    internal const string NONE = "NONE";
    internal const int MAGIC_SEED = 1337;
    internal static readonly JsonSerializerOptions SerializerOption = new()
    {
        IgnoreReadOnlyFields = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull | System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
        PropertyNamingPolicy = new LowerCaseNamingPolicy()
    };
    private class LowerCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToLower();
    }
    internal static readonly OpenAIRequestBody defaultRequestParam = new();
}