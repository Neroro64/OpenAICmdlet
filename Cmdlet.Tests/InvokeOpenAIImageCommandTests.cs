﻿namespace OpenAICmdlet.Tests;

[TestClass]
public class InvokeOpenAIImageCommandTests
{
    [TestMethod]
    public void CanBindParametersWhatIf()
    {
        using var ps = PowerShellTestBase.CreatePowerShell("Invoke-OpenAIImage",
                                                           typeof(InvokeOpenAIImageCommand));
        using var mockMsgHandler = new MockHandler(
            (request) =>
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(MockOpenAIResponseData.ImageResponse),
                };
            });

        WebRequest.AddHttpClient(SecureAPIKey.DefaultAPIKeyPath, mockMsgHandler, "abcd1234");

        ps!.AddCommand("Invoke-OpenAIImage")
            .AddParameter("Prompt", "Hello World")
            .AddParameter("Mode", OpenAITask.ImageGeneration)
            .AddParameter("ImageSize", "1024x1024")
            .AddParameter("ImagePath", "../../../../docs/resources/a_happy_man_eating_hot_dog.png")
            .AddParameter("ImageMaskPath", "../../../../docs/resources/a_happy_man_eating_hot_dog.png")
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
        using var ps = PowerShellTestBase.CreatePowerShell("Invoke-OpenAIImage",
                                                           typeof(InvokeOpenAIImageCommand));
        ArgumentNullException.ThrowIfNull(param);
        string content = MockOpenAIResponseData.ImageResponse;

        using var mockMsgHandler = new MockHandler(
            (request) =>
            {
                return new HttpResponseMessage(
                    System.Net.HttpStatusCode.OK)
                { Content = new StringContent(content) };
            });

        WebRequest.AddHttpClient(SecureAPIKey.DefaultAPIKeyPath, mockMsgHandler, "abcd1234");

        ps!.AddCommand("Invoke-OpenAIImage");
        foreach (var kv in param)
            ps.AddParameter(kv.Key, kv.Value);

        var result = ps.Invoke<Response>().ToList();
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Count, 1);

        // Verify response content
        Assert.IsTrue(result.First().Body.First().Contains(
            "https", StringComparison.InvariantCultureIgnoreCase));
    }

    [TestMethod]
    [Ignore]
    public void CanInvokeCommandForReal()
    {
        using var ps = PowerShellTestBase.CreatePowerShell("Invoke-OpenAIImage",
                                                           typeof(InvokeOpenAIImageCommand));

        ps!.AddCommand("Invoke-OpenAIImage")
            .AddParameter("Mode", OpenAITask.ImageVariation)
            .AddParameter("ImagePath", "../../../../OpenAICmdlet/resources/logo.png");

        var result = ps.Invoke<Response>().ToList();
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Count, 1);
    }

    public static IEnumerable<object[]> ParamSet
    {
        get
        {
            return new[] {
                new object[] { new Dictionary<string, object>() { ["Prompt"] = "Hello World" } },
                new object[] { new Dictionary<string, object>() {
                    ["Prompt"] = "Hello World",
                    ["Mode"] = OpenAITask.ImageEdit,
                    ["ImageSize"] = "512x512",
                    ["ImagePath"] = "../../../../docs/resources/a_happy_man_eating_hot_dog.png",
                    ["ImageMaskPath"] = "../../../../docs/resources/a_happy_man_eating_hot_dog.png",
                    ["Samples"] = 1234,
                } },
                new object[] { new Dictionary<string, object>() {
                    ["Mode"] = OpenAITask.ImageVariation,
                    ["ImageSize"] = "512x512",
                    ["ImagePath"] = "../../../../docs/resources/a_happy_man_eating_hot_dog.png",
                    ["Samples"] = 1234,
                } }
            };
        }
    }
}
