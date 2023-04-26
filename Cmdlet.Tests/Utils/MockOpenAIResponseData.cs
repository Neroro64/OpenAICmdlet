namespace OpenAICmdlet.Tests;
public static class MockOpenAIResponseData
{
    public const string CompletionResponse = @"
{
  ""content"": { ""id"": ""cmpl-uqkvlQyYK7bGYrRHQ0eXlWi7"", ""object"": ""text_completion"", ""created"": 1589478378, ""model"": ""text-davinci-003"", ""choices"": [ { ""text"": ""\n\nThis is indeed a test"", ""index"": 0, ""logprobs"": null, ""finish_reason"": ""length"" } ], ""usage"": { ""prompt_tokens"": 5, ""completion_tokens"": 7, ""total_tokens"": 12 } }
}
";
    public const string CompletionResponseText = "\n\nThis is indeed a test";

    public const string ChatResponse = @"
{
  ""content"": { ""id"": ""chatcmpl-123"", ""object"": ""chat.completion"", ""created"": 1677652288, ""choices"": [ { ""index"": 0, ""message"": { ""role"": ""assistant"", ""content"": ""\n\nHello there, how may I assist you today?"" }, ""finish_reason"": ""stop"" } ], ""usage"": { ""prompt_tokens"": 9, ""completion_tokens"": 12, ""total_tokens"": 21 } }
}
";
    public const string ChatResponseText = "\n\nHello there, how may I assist you today?";

    public const string ImageResponse = @"
{
  ""content"": { ""created"": 1589478378, ""data"": [ { ""url"": ""https://..."" }, { ""url"": ""https://..."" } ] }
}
";
    public const string AudioResponse = @"
{
  ""content"": { ""text"": ""Hello, my name is Wolfgang and I come from Germany. Where are you heading today?"" }
}
";
}