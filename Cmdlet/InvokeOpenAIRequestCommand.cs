using System.Text.Json;
namespace OpenAICmdlet;

[Cmdlet(VerbsLifecycle.Invoke, "OpenAIRequest", SupportsShouldProcess = true)]
[OutputType(typeof(IOpenAIResponse))]
public class InvokeOpenAIRequestCommand<T> : MyCmdlet
    where T : IOpenAIResponse, new()
{
    [Parameter(Mandatory = true, ValueFromPipeline = true,
               ValueFromPipelineByPropertyName = true)]
    public Uri EndPoint { get; init; } = OpenAIEndpoint.Default;

    [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
    public Dictionary<string, object>? Body { get; init; }

    [Parameter()]
    public string? APIKeyPath { get; init; }

    public InvokeOpenAIRequestCommand()
    {
        string apiKey = GetAPIKey();
        WebRequest.HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
    }
    protected override void EndProcessing()
    {
        string content = JsonSerializer.Serialize(Body);
        string shouldProcessDesc = $@"
        Sending a request to {EndPoint.AbsoluteUri} with
        ---
        Header : {WebRequest.HttpClient.DefaultRequestHeaders.ToString()}
        ---
        Content : {content}
        ---
            @";
        if (ShouldProcess(shouldProcessDesc, shouldProcessDesc, "Invoke OpenAI API request"))
        {
            try
            {
                CancellationTokenSource cancellationToken = new();
                WriteObject(WebRequest.InvokeAsync<T>(EndPoint, HttpMethod.Post,
                                                     new StringContent(content),
                                                     cancellationToken.Token).Result);
            }
            catch (Exception exp)
            {
                throw exp.InnerException ?? new HttpRequestException($"Http Request failed with message: {exp.Message}");
            }
        }
    }
    public virtual string GetAPIKey() => SecureAPIKey.DecryptFromFile(APIKeyPath);
}
