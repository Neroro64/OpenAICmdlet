using System.Management.Automation;
using System.Net.Http.Json;
namespace OpenAICmdlet.Tests;

[TestClass]
public class OpenAIHistoryTests
{
    public static readonly string localAppDataPath = System.IO.Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OpenAICmdlet");

    [ClassCleanup]
    public static void ClearLocalFiles()
    {
        if (Directory.Exists(localAppDataPath))
            Directory.Delete(localAppDataPath, recursive: true);
    }

    [TestMethod]
    [DynamicData(nameof(ParamSet))]
    public void CanGetAndSetHistory(Dictionary<string, object> param)
    {
        // Dry-run of Invoke-OpenAIText
        using var ps = PowerShellTestBase.CreatePowerShell(new[] { "Invoke-OpenAIText", "Get-OpenAIResponseHistory", "Backup-OpenAIResponseHistory" },
                                                           new[] { typeof(InvokeOpenAITextCommand), typeof(GetOpenAIResponseHistoryCommand), typeof(BackupOpenAIResponseHistory) });
        ArgumentNullException.ThrowIfNull(param);
        (string content, string response) =
            (param.ContainsKey("Mode"))
                ? (((OpenAITask)param["Mode"] == OpenAITask.TextCompletion)
                       ? (MockOpenAIResponseData.CompletionResponse,
                          MockOpenAIResponseData.CompletionResponseText)
                       : (MockOpenAIResponseData.ChatResponse,
                          MockOpenAIResponseData.ChatResponseText))
                : (MockOpenAIResponseData.ChatResponse, MockOpenAIResponseData.ChatResponseText);

        using var mockMsgHandler = new WebRequest.MockHandler(
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

        var result = ps.Invoke<OpenAIResponse>().ToList();
        Assert.IsNotNull(result);

        // Test Get
        ps.Commands = new();
        ps.AddCommand("Get-OpenAIResponseHistory").AddParameter("CommandCategory", OpenAICategory.Text);
        var history = ps.Invoke<IEnumerable<IEnumerable<OpenAIResponse>>>().ToList();
        Assert.IsNotNull(history);
        Assert.IsTrue(history.Count > 0);

        // Test Backup
        ps.Commands = new();
        ps.AddCommand("Backup-OpenAIResponseHistory").AddParameter("CommandCategory", OpenAICategory.Text);
        var output = ps.Invoke().ToList();
        Assert.IsTrue(Directory.GetFiles(System.IO.Path.Join(localAppDataPath, "SessionHistory")).Length > 0);
    }

    public static IEnumerable<object[]> ParamSet
    {
        get
        {
            return new[] {
                new object[] { new Dictionary<string, object>() { ["Prompt"] = "Hello World" } }
            };
        }
    }
}
