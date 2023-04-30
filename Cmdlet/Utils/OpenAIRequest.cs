using System.Net.Http.Json;
using System.Net.Http.Headers;
namespace OpenAICmdlet;

public class OpenAIRequest
{
    public Uri EndPoint { get; init; }

    public OpenAIRequestBody Body { get; init; }
    public bool UploadFile { get; init; }

    public string APIKeyPath { get; init; }

    public OpenAIRequest(Uri endPoint, OpenAIRequestBody body, string? apiKeyPath, bool uploadFile = false)
    {
        if (!typeof(OpenAIRequestBody).IsSerializable)
            throw new InvalidOperationException(
                "(OpenAIRequestBody) Request content type must be serializable!");
        EndPoint = endPoint;
        Body = body;
        APIKeyPath = apiKeyPath ?? SecureAPIKey.DefaultAPIKeyPath;
        UploadFile = uploadFile;
    }

    public async Task<JsonNode?> InvokeAsync(CancellationToken cancellationToken,
                                             HttpMessageHandler? messageHandler = null)
    {
        if (cancellationToken.IsCancellationRequested)
            return null;

        using JsonContent content = JsonContent.Create<OpenAIRequestBody>(
            Body, new MediaTypeWithQualityHeaderValue("application/json"),
            Constant.SerializerOption);

        using MultipartFormDataContent formContent = new();
        if (UploadFile)
        {
            formContent.Add(content);

            if (Body.Image != null)
                formContent.AddFileContent("image", Body.Image);
            if (Body.Mask != null)
                formContent.AddFileContent("mask", Body.Mask);
            if (Body.File != null)
                formContent.AddFileContent("file", Body.File);
        }
        try
        {
            var httpClient = WebRequest.GetHttpClient(APIKeyPath) ??
                             WebRequest.AddHttpClient(APIKeyPath, messageHandler,
                                                      SecureAPIKey.DecryptFromFile(APIKeyPath));
            return await httpClient
                .InvokeAsync(EndPoint, HttpMethod.Post, (UploadFile) ? formContent : content, cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: true);
        }
        catch (Exception exp)
        {
            throw exp.InnerException ??
                new HttpRequestException($"Http Request failed with message: {exp.Message}");
        }
    }

}
