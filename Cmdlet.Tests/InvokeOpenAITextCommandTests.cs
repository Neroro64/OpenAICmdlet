using System.Management.Automation;
using System.Net.Http.Json;
namespace OpenAICmdlet.Tests;

[TestClass]
public class InvokeOpenAITextCommandTests
{
    [TestMethod]
    public void CanBindParametersWhatIf()
    {
        using var ps = PowerShellTestBase.CreatePowerShell("Invoke-OpenAIText",
                                                           typeof(InvokeOpenAITextCommand));
        using var mockMsgHandler = new MockHandler(
            (request) =>
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(MockOpenAIResponseData.CompletionResponse),
                };
            });

        WebRequest.AddHttpClient(SecureAPIKey.DefaultAPIKeyPath, mockMsgHandler, "abcd1234");

        ps!.AddCommand("Invoke-OpenAIText")
            .AddParameter("Prompt", "Hello World")
            .AddParameter("Mode", OpenAITask.TextCompletion)
            .AddParameter("ContextFilePath", "../../../Resources/MockContextFile.txt")
            .AddParameter("Suffix", ";)")
            .AddParameter("MaxTokens", 4096)
            .AddParameter("Temperatur", 1f)
            .AddParameter("Top_p", 1f)
            .AddParameter("PrecencePenalty", 2f)
            .AddParameter("FrequencyPenalty", 2f)
            .AddParameter("ChatInitInstruction", "This is a test")
            .AddParameter("StopSequences", new string[] { "\n", "\r" })
            .AddParameter("Samples", 1234)
            .AddParameter("WhatIf");

        var result = ps.Invoke().ToList();
        Assert.IsNotNull(result);
        Assert.IsFalse(ps.HadErrors);
        Assert.IsTrue(result.Count == 0);
    }

    [TestMethod]
    [DynamicData(nameof(ParamSet))]
    public void CanInvokeCommand(Dictionary<string, object> param)
    {
        using var ps = PowerShellTestBase.CreatePowerShell("Invoke-OpenAIText",
                                                           typeof(InvokeOpenAITextCommand));
        ArgumentNullException.ThrowIfNull(param);
        (string content, string response) =
            (param.ContainsKey("Mode"))
                ? (((OpenAITask)param["Mode"] == OpenAITask.TextCompletion)
                       ? (MockOpenAIResponseData.CompletionResponse,
                          MockOpenAIResponseData.CompletionResponseText)
                       : (MockOpenAIResponseData.ChatResponse,
                          MockOpenAIResponseData.ChatResponseText))
                : (MockOpenAIResponseData.ChatResponse, MockOpenAIResponseData.ChatResponseText);

        using var mockMsgHandler = new MockHandler(
            (request) =>
            {
                return new HttpResponseMessage(
                    System.Net.HttpStatusCode.OK)
                { Content = new StringContent(content) };
            });

        WebRequest.AddHttpClient(SecureAPIKey.DefaultAPIKeyPath, mockMsgHandler, "abcd1234");

        ps!.AddCommand("Invoke-OpenAIText");
        foreach (var kv in param)
            ps.AddParameter(kv.Key, kv.Value);

        var result = ps.Invoke<Response>().ToList();
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Count, 1);

        // Verify response content
        Assert.AreEqual(result.First().Body.First(), response);
    }

    [TestMethod]
    public void CanContinueMultipleSession()
    {
        using var ps = PowerShellTestBase.CreatePowerShell("Invoke-OpenAIText",
                                                           typeof(InvokeOpenAITextCommand));
        (string textContent, string textResponse) = (MockOpenAIResponseData.CompletionResponse,
                                                     MockOpenAIResponseData.CompletionResponseText);
        (string chatContent, string chatResponse) =
            (MockOpenAIResponseData.ChatResponse, MockOpenAIResponseData.ChatResponseText);

        bool checkForLength = false;
        bool useChat = false;
        using var mockMsgHandler = new MockHandler(
            (request) =>
            {
                if (checkForLength)
                    Assert.IsTrue(((RequestBody?)(request.Content as JsonContent)?.Value)
                                      ?.Messages?.Count() == 4);

                if (useChat)
                    return new HttpResponseMessage(
                        System.Net.HttpStatusCode.OK)
                    { Content = new StringContent(chatContent) };
                else
                    return new HttpResponseMessage(
                        System.Net.HttpStatusCode.OK)
                    { Content = new StringContent(textContent) };
            });

        WebRequest.AddHttpClient(SecureAPIKey.DefaultAPIKeyPath, mockMsgHandler, "abcd1234");

        // First session
        ps!.AddCommand("Invoke-OpenAIText")
            .AddParameter("Prompt", "Hello")
            .AddParameter("Mode", OpenAITask.TextCompletion)
            .AddParameter("ContextFilePath", "../../../Resources/MockContextFile.txt");

        var result1 = ps.Invoke<Response>().ToList();
        Assert.IsNotNull(result1);
        Assert.IsFalse(ps.HadErrors);
        Assert.AreEqual(result1.First().Body.First(), textResponse);
        ps.Commands = new PSCommand();

        // Continue first session
        useChat = true;
        checkForLength = true;
        ps!.AddCommand("Invoke-OpenAIText")
            .AddParameter("Prompt", "World")
            .AddParameter("Mode", OpenAITask.ChatCompletion)
            .AddParameter("ContinueSession", result1);

        var result2 = ps.Invoke<Response>().ToList();
        Assert.IsNotNull(result2);
        Assert.IsFalse(ps.HadErrors);
        Assert.AreEqual(result2.First().Body.First(), chatResponse);
    }

    [TestMethod]
    public void CanInvokeCommandThroughPipeline()
    {
        using var ps = PowerShellTestBase.CreatePowerShell("Invoke-OpenAIText",
                                                           typeof(InvokeOpenAITextCommand));
        (string content, string response) = (MockOpenAIResponseData.CompletionResponse,
                                             MockOpenAIResponseData.CompletionResponseText);

        using var mockMsgHandler = new MockHandler(
            (request) =>
            {
                return new HttpResponseMessage(
                    System.Net.HttpStatusCode.OK)
                { Content = new StringContent(content) };
            });

        WebRequest.AddHttpClient(SecureAPIKey.DefaultAPIKeyPath, mockMsgHandler, "abcd1234");

        ps!.AddScript("\"Hello World\"")
            .AddCommand("Invoke-OpenAIText")
            .AddParameter("Mode", OpenAITask.TextCompletion)
            .AddParameter("ContextFilePath", "../../../Resources/MockContextFile.txt");

        var result = ps.Invoke<Response>().ToList();
        Assert.IsNotNull(result);
        Assert.IsFalse(ps.HadErrors);
        Assert.AreEqual(result.First().Body.First(), response);
    }

    [TestMethod]
    public void CanInvokeCommandThroughPipelineUsingPropertyName()
    {
        using var ps = PowerShellTestBase.CreatePowerShell("Invoke-OpenAIText",
                                                           typeof(InvokeOpenAITextCommand));
        (string content, string response) = (MockOpenAIResponseData.CompletionResponse,
                                             MockOpenAIResponseData.CompletionResponseText);

        using var mockMsgHandler = new MockHandler(
            (request) =>
            {
                return new HttpResponseMessage(
                    System.Net.HttpStatusCode.OK)
                { Content = new StringContent(content) };
            });

        WebRequest.AddHttpClient(SecureAPIKey.DefaultAPIKeyPath, mockMsgHandler, "abcd1234");

        ps!.AddScript(
               "[PSCustomObject]@{Prompt=\"Hello World\"; ContextFilePath= \"../../../Resources/MockContextFile.txt\"}")
            .AddCommand("Invoke-OpenAIText")
            .AddParameter("Mode", OpenAITask.TextCompletion);

        var result = ps.Invoke<Response>().ToList();
        Assert.IsNotNull(result);
        Assert.IsFalse(ps.HadErrors);
        Assert.AreEqual(result.First().Body.First(), response);
    }

    [TestMethod]
    [Ignore]
    public void CanInvokeCommandForReal()
    {
        using var ps = PowerShellTestBase.CreatePowerShell("Invoke-OpenAIText",
                                                           typeof(InvokeOpenAITextCommand));
        ps!.AddCommand("Invoke-OpenAIText").AddParameter("Prompt", "This is a test");

        var result = ps.Invoke<Response>().ToList();
        Assert.IsNotNull(result);
        Assert.IsFalse(ps.HadErrors);
    }

    public static IEnumerable<object[]> ParamSet
    {
        get
        {
            return new[] {
                new object[] { new Dictionary<string, object>() { ["Prompt"] = "Hello World" } },
                new object[] { new Dictionary<string, object>() { ["Prompt"] = "Hello World",
                                                                  ["Mode"] =
                                                                      OpenAITask.TextCompletion } },
                new object[] { new Dictionary<string, object>() {
                    ["Prompt"] = "Hello World",
                    ["ContextFilePath"] = "../../../Resources/MockContextFile.txt",
                    ["Suffix"] = ";)",
                    ["MaxTokens"] = 4096,
                    ["Temperatur"] = 1f,
                    ["Top_p"] = 1f,
                    ["PrecencePenalty"] = 2f,
                    ["FrequencyPenalty"] = 2f,
                    ["ChatInitInstruction"] = "This is a test",
                    ["StopSequences"] = new string[] { "\n", "\r" },
                    ["Samples"] = 1234,
                } },
                new object[] { new Dictionary<string, object>() {
                    ["Prompt"] = "Hello World",
                    ["Mode"] = OpenAITask.TextCompletion,
                    ["ContextFilePath"] = "../../../Resources/MockContextFile.txt",
                    ["Suffix"] = ";)",
                    ["MaxTokens"] = 4096,
                    ["Temperatur"] = 1f,
                    ["Top_p"] = 1f,
                    ["PrecencePenalty"] = 2f,
                    ["FrequencyPenalty"] = 2f,
                    ["ChatInitInstruction"] = "This is a test",
                    ["StopSequences"] = new string[] { "\n", "\r" },
                    ["Samples"] = 1234,
                } }
            };
        }
    }
}
