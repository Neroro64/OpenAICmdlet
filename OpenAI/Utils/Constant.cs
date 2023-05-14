namespace OpenAI;
public static class Constant
{
    public const int HTTP_TIMEOUT_MIN = 5;
    public const int RW_LOCK_TIMEOUT_MS = 100;
    public const string NONE = "NONE";
    public const int MAGIC_SEED = 1337;
    public static readonly JsonSerializerOptions SerializerOption =
        new()
        {
            IgnoreReadOnlyFields = true,
            DefaultIgnoreCondition =
                    System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
            PropertyNamingPolicy = new LowerCaseNamingPolicy()
        };
    private sealed class LowerCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToLowerInvariant();
    }
    public static readonly RequestBody defaultRequestParam = new();
}
