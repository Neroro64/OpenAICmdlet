namespace OpenAICmdlet.Tests;

[TestClass]
public class InvokeOpenAIAudioCommandTests
{
    [TestMethod]
    public void CanBindParametersWhatIf()
    {
        using var ps = PowerShellTestBase.CreatePowerShell("Invoke-OpenAIAudio", typeof(InvokeOpenAIAudioCommand));
        using var mockMsgHandler = new WebRequest.MockHandler(
            (request) =>
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(MockOpenAIResponseData.ImageResponse),
                };
            });

        WebRequest.AddHttpClient(SecureAPIKey.DefaultAPIKeyPath, mockMsgHandler, "abcd1234");

        ps!.AddCommand("Invoke-OpenAIAudio")
            .AddParameter("Prompt", "Hello world")
            .AddParameter("Mode", OpenAITask.AudioTranscription)
            .AddParameter("AudioPath", "../../../../resources/f7879738_nohash_0.wav")
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
        using var ps = PowerShellTestBase.CreatePowerShell("Invoke-OpenAIAudio", typeof(InvokeOpenAIAudioCommand));
        ArgumentNullException.ThrowIfNull(param);
        (string content, string response) = (MockOpenAIResponseData.AudioResponse, MockOpenAIResponseData.AudioResponseText);

        using var mockMsgHandler = new WebRequest.MockHandler(
            (request) =>
            {
                return new HttpResponseMessage(
                    System.Net.HttpStatusCode.OK)
                { Content = new StringContent(content) };
            });

        WebRequest.AddHttpClient(SecureAPIKey.DefaultAPIKeyPath, mockMsgHandler, "abcd1234");

        ps!.AddCommand("Invoke-OpenAIAudio");
        foreach (var kv in param)
            ps.AddParameter(kv.Key, kv.Value);

        var result = ps.Invoke<OpenAIResponse>().ToList();
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Count, 1);

        // Verify response content
        Assert.AreEqual(result.First().Response.First(), response);
    }

    public static IEnumerable<object[]> ParamSet
    {
        get
        {
            return new[] {
                new object[] { new Dictionary<string, object>() { ["AudioPath"] = "../../../../resources/f7879738_nohash_0.wav" } },
                new object[] { new Dictionary<string, object>() {
                    ["Prompt"] = "Hello World",
                    ["AudioLanguage"] = "en",
                    ["Mode"] = OpenAITask.AudioTranscription,
                    ["AudioPath"] = "../../../../resources/f7879738_nohash_0.wav",
                } },
                new object[] { new Dictionary<string, object>() {
                    ["Prompt"] = "Hello World",
                    ["AudioLanguage"] = "en",
                    ["Mode"] = OpenAITask.AudioTranslation,
                    ["AudioPath"] = "../../../../resources/f7879738_nohash_0.wav",
                    ["Temperature"] = 2f
                } }
            };
        }
    }
}
