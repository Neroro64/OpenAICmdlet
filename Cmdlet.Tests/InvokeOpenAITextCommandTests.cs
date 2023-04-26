using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text.Json.Nodes;
namespace OpenAICmdlet.Tests;

[TestClass]
public class InvokeOpenAITextCommandTests
{
    PowerShell? ps;
    [TestInitialize]
    public void TestInitialize()
    {
        var initialSessionState = InitialSessionState.CreateDefault();
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Invoke-OpenAIText", typeof(InvokeOpenAITextCommand), null));

        ps = PowerShell.Create(initialSessionState);
    }
    [TestMethod]
    public void CanBindParametersWhatIf()
    {

        var mockMsgHandler = new WebRequest.MockHandler((request) =>
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
        (string content, string response) =
        (param.ContainsKey("Mode")) ?
            (
                ((OpenAITask)param["Mode"] == OpenAITask.TextCompletion)
                ? (MockOpenAIResponseData.CompletionResponse, MockOpenAIResponseData.CompletionResponseText) : (MockOpenAIResponseData.ChatResponse, MockOpenAIResponseData.ChatResponseText)
            ) : (MockOpenAIResponseData.ChatResponse, MockOpenAIResponseData.ChatResponseText);

        var mockMsgHandler = new WebRequest.MockHandler((request) =>
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            };
        });

        WebRequest.AddHttpClient(SecureAPIKey.DefaultAPIKeyPath, mockMsgHandler, "abcd1234");

        ps!.AddCommand("Invoke-OpenAIText");
        foreach (var kv in param)
            ps.AddParameter(kv.Key, kv.Value);

        var result = ps.Invoke<OpenAIResponse>().ToList();
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Count, 1);

        // Verify response content
        Assert.AreEqual(result.First().Response.First(), response);
    }

    [TestMethod]
    public void CanContinueChat() { }

    [TestMethod]
    public void CanContinueMultipleSession() { }

    [TestMethod]
    public void CanInvokeCommandThroughPipeline() { }

    public static IEnumerable<object[]> ParamSet
    {
        get
        {
            return new[]
            {
                new object[] { new Dictionary<string, object>(){ ["Prompt"] = "Hello World" }},
                new object[] { new Dictionary<string, object>()
                {
                    ["Prompt"] = "Hello World",
                    ["Mode"] = OpenAITask.TextCompletion
                }},
                new object[] { new Dictionary<string, object>()
                {
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
                }},
                new object[] { new Dictionary<string, object>()
                {
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
                }}
            };
        }
    }
}
