namespace OpenAICmdlet.Tests;

[TestClass]
public class OpenAIRequestTests
{
    [TestMethod]
    public void CanInvokeRequest()
    {
        using var mockMsgHandler = new WebRequest.MockHandler((request) =>
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(MockOpenAIResponseData.CompletionResponse),
            };
        });

        WebRequest.AddHttpClient(SecureAPIKey.DefaultAPIKeyPath, mockMsgHandler, "abcd1234");
        var mockRequest = new Mock<OpenAIRequest>(OpenAIEndpoint.Default, new OpenAIRequestBody()
        {
            Prompt = "Hello World",
            Messages = new List<Dictionary<string, string>>()
            {
                new()
                {
                    ["role"] = "system",
                    ["content"] = "Helpful Assistant"
                },
                new()
                {
                    ["role"] = "user",
                    ["content"] = "Test prompt"
                },
            },
            Stop = new string[] { "a", "\r", "\n" }
        }, null, false)
        { CallBase = true };

        var request = mockRequest.Object;

        var result = request.InvokeAsync(CancellationToken.None).Result;
        Assert.IsNotNull(result);
        Assert.AreEqual(result["content"]?["choices"]?.AsArray().Count, 1);
    }
}
