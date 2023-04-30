using System.Net.Http.Json;
using System.Net.Http.Headers;
namespace OpenAICmdlet;

public class OpenAIRequest
{
    public Uri EndPoint { get; init; }

    public OpenAIRequestBody Body { get; init; }
    public bool UploadFile { get; init; }

    public string APIKeyPath { get; init; }
    private List<IDisposable> disposables = new List<IDisposable>();

    public OpenAIRequest(Uri endPoint, OpenAIRequestBody body, string? apiKeyPath,
                         bool uploadFile = false)
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

        if (UploadFile)
        {
            var addFileContent = (MultipartFormDataContent formContent, string name,
                                  string filePath, string mediaType) =>
            {
                if (formContent == null)
                    return;
                FileStream fs = File.OpenRead(filePath);
                StreamContent content = new StreamContent(fs);
                if (!String.IsNullOrEmpty(mediaType))
                    content.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType);
                formContent.Add(content, name, filePath);

                disposables.Add(fs);
                disposables.Add(content);
            };

            MultipartFormDataContent content = new();
            disposables.Add(content);

            if (Body.Image != null)
                addFileContent(content, "image", Body.Image, $"image/{getFileType(Body.Image)}");
            if (Body.Mask != null)
                addFileContent(content, "mask", Body.Mask, $"image/{getFileType(Body.Mask)}");
            if (Body.File != null)
                addFileContent(content, "file", Body.File, $"audio/{getFileType(Body.File)}");

            var jsNode =
                JsonNode.Parse(JsonSerializer.Serialize(Body, options: Constant.SerializerOption));
            if (jsNode != null)
            {
                foreach (var kv in jsNode.AsObject())
                {
                    if (kv.Value != null)
                    {
                        var strContent = new StringContent(kv.Value.ToString());
                        content.Add(strContent, kv.Key);
                        disposables.Add(strContent);
                    }
                }
            }
            if (content.Count() == 1)
                throw new ArgumentException("No files were provided for upload");

            try
            {
                var httpClient = WebRequest.GetHttpClient(APIKeyPath) ??
                                 WebRequest.AddHttpClient(APIKeyPath, messageHandler,
                                                          SecureAPIKey.DecryptFromFile(APIKeyPath));
                var response =
                    await httpClient
                        .InvokeAsync(EndPoint, HttpMethod.Post, content, cancellationToken)
                        .ConfigureAwait(continueOnCapturedContext: true);

                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }
        else
        {
            JsonContent content = JsonContent.Create<OpenAIRequestBody>(
                Body, new MediaTypeWithQualityHeaderValue("application/json"),
                Constant.SerializerOption);
            disposables.Add(content);
            try
            {
                var httpClient = WebRequest.GetHttpClient(APIKeyPath) ??
                                 WebRequest.AddHttpClient(APIKeyPath, messageHandler,
                                                          SecureAPIKey.DecryptFromFile(APIKeyPath));
                var response =
                    await httpClient
                        .InvokeAsync(EndPoint, HttpMethod.Post, content, cancellationToken)
                        .ConfigureAwait(continueOnCapturedContext: true);
                return response;
            }
            catch (Exception exp)
            {
                throw exp.InnerException ??
                    new HttpRequestException($"Http Request failed with message: {exp.Message}");
            }
        }
    }
    private static string getFileType(string filePath) => Path.GetExtension(filePath).Trim('.');
    ~OpenAIRequest()
    {
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}
