using System.Net.Http.Json;
using System.Net.Http.Headers;
namespace OpenAICmdlet;

public class OpenAIRequest<TContent> where TContent : new()
{
    public Uri EndPoint { get; init; }

    public TContent Body { get; init; }

    public string APIKeyPath { get; init; }

    public OpenAIRequest(Uri endPoint, TContent body, string? apiKeyPath)
    {
        if (!typeof(TContent).IsSerializable)
            throw new InvalidOperationException("(TContent) Request content type must be serializable!");
        EndPoint = endPoint;
        Body = body;
        APIKeyPath = apiKeyPath ?? SecureAPIKey.DefaultAPIKeyPath;
    }

    public async Task<JsonNode?> InvokeAsync(CancellationToken cancellationToken, HttpMessageHandler? messageHandler = null)
    {
        if (cancellationToken.IsCancellationRequested)
            return null;

        JsonContent content = JsonContent.Create<TContent>(
            Body,
            new MediaTypeWithQualityHeaderValue("application/json"),
            Constant.SerializerOption);
        try
        {
            var httpClient =
            WebRequest.GetHttpClient(APIKeyPath)
            ?? WebRequest.AddHttpClient(APIKeyPath, messageHandler, SecureAPIKey.DecryptFromFile(APIKeyPath));
            return await httpClient.InvokeAsync(EndPoint, HttpMethod.Post, content, cancellationToken);
        }
        catch (Exception exp)
        {
            throw exp.InnerException ?? new HttpRequestException($"Http Request failed with message: {exp.Message}");
        }
    }
}
