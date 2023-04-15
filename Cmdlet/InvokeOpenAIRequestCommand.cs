using System.Text.Json;
using System.Net;
using System.Net.Http.Json;
namespace OpenAICmdlet;

[Cmdlet(VerbsLifecycle.Invoke, "OpenAIRequest", SupportsShouldProcess = true)]
[OutputType(typeof(OpenAIResponse))]
public class InvokeOpenAIRequestCommand<T> : MyCmdlet
    where T : OpenAIResponse, new()
{
    public static HttpClient httpClient =
        new() { Timeout = TimeSpan.FromMinutes(5) };
    [Parameter(ParameterSetName = "usingBody")]
    [Parameter(ParameterSetName = "usingForm")]
    [Parameter(Mandatory = true, ValueFromPipeline = true,
               ValueFromPipelineByPropertyName = true)]
    public Uri EndPoint { get; init; } = new("https://api.openai.com/v1");

    [Parameter(ParameterSetName = "usingBody")]
    public Dictionary<string, object>? Body { get; init; } = new Dictionary<string, object>() { };

    [Parameter(ParameterSetName = "usingForm")]
    public Dictionary<string, object>? Form { get; init; }

    [Parameter(ParameterSetName = "usingBody")]
    [Parameter(ParameterSetName = "usingForm")]
    public string? APIKeyPath { get; init; }

    public InvokeOpenAIRequestCommand()
    {
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
        );

        string apiKey = GetAPIKey();
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
    }
    protected override void EndProcessing()
    {
        string content = (Body != null) ? JsonSerializer.Serialize(Body)
                                        : JsonSerializer.Serialize(Form);
        string shouldProcessDesc = $@"
        Sending a request to {EndPoint.AbsoluteUri} with
        ---
        Header : {httpClient.DefaultRequestHeaders.ToString()}
        ---
        Content : {content}
        ---
            @";
        if (ShouldProcess(shouldProcessDesc, shouldProcessDesc,
                          "Invoke OpenAI API request"))
        {
            try
            {
                CancellationTokenSource cancellationToken = new();
                WriteObject(InvokeWebRequestAsync<T>(EndPoint, HttpMethod.Post,
                                                     new StringContent(content),
                                                     cancellationToken.Token).Result);
            }
            catch (Exception exp)
            {
                throw exp.InnerException ?? new HttpRequestException($"Http Request failed with message: {exp.Message}");
            }
        }
    }
    public static async Task<ReturnType?> InvokeWebRequestAsync<ReturnType>(
        Uri endpoint, HttpMethod method, StringContent? content,
        CancellationToken cancellationToken = default)
        where ReturnType : new()
        => method.Method switch
        {
            WebRequestMethods.Http.Get => await httpClient.GetFromJsonAsync<ReturnType>(endpoint, cancellationToken),
            WebRequestMethods.Http.Post or WebRequestMethods.Http.Put => await Task.Run<ReturnType>(() =>
            {
                var response = httpClient.PostAsync(endpoint, content, cancellationToken).Result;
                response.EnsureSuccessStatusCode();
                ReturnType result = response.Content.ReadFromJsonAsync<ReturnType>().Result ?? new ReturnType();
                return result;
            }),
            _ => throw new ArgumentException("Invalid HTTP Method provided!")
        };
    public virtual string GetAPIKey() => GetOpenAIAPIKeyCommand.GetDecryptedAPIKey(APIKeyPath);
}
