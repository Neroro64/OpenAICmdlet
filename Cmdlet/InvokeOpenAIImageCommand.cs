namespace OpenAICmdlet;

[Cmdlet(VerbsLifecycle.Invoke, "OpenAIImage", SupportsShouldProcess = true)]
[Alias("dalle")]
[OutputType(typeof(OpenAIResponse))]
public class InvokeOpenAIImageCommand : MyCmdlet
{
    private static List<List<OpenAIResponse>> _history = new();

    [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true,
    HelpMessage = "The prompt(s) to generate images or guide image edits")]
    [ValidateLength(0, 4096)]
    public string Prompt { get; set; } = String.Empty;

    [Parameter(HelpMessage = "Image generation mode")]
    [ValidateSet(nameof(OpenAITask.ImageGeneration), nameof(OpenAITask.ImageEdit), nameof(OpenAITask.ImageVariation))]
    public OpenAITask Mode { get; set; } = OpenAITask.ImageGeneration;

    [Parameter(HelpMessage = "Path to the input image file (png)")]
    [ValidatePattern(".png$")]
    public string ImagePath { get; set; } = String.Empty;

    [Parameter(HelpMessage = "The size of the generated image")]
    [ValidateSet("256x256", "512x512", "1024x1024")]
    public string ImageSize { get; set; } = "256x256";

    [Parameter(HelpMessage = "Path to the image mask file")]
    [ValidatePattern(".png$")]
    public string ImageMaskPath { get; set; } = String.Empty;

    [Parameter(HelpMessage = "Number of images that should be generated")]
    public int Samples { get; set; } = Constant.defaultRequestParam.N;

    [Parameter(HelpMessage = "Path to OpenAI API key file")]
    public string? APIKeyPath { get; set; }

    private OpenAIRequestBody _requestBody = new();
    protected override void BeginProcessing()
    {
        _requestBody = new()
        {
            Model = OpenAIModel.TaskModel(Mode),
            N = Samples,
            Size = ImageSize
        };
    }
    protected override void ProcessRecord()
    {
        bool uploadFile = false;
        switch (Mode)
        {
            case OpenAITask.ImageGeneration:
                ArgumentNullException.ThrowIfNullOrEmpty(Prompt);
                _requestBody.Prompt = Prompt;
                break;
            case OpenAITask.ImageEdit:
                ArgumentNullException.ThrowIfNullOrEmpty(Prompt);
                ArgumentNullException.ThrowIfNullOrEmpty(ImagePath);
                ArgumentNullException.ThrowIfNullOrEmpty(ImageMaskPath);
                ImagePath.ThrowIfFileNotFound();
                ImageMaskPath.ThrowIfFileNotFound();

                _requestBody.Prompt = Prompt;
                _requestBody.Image = ImagePath;
                _requestBody.Mask = ImageMaskPath;
                uploadFile = true;
                break;
            case OpenAITask.ImageVariation:
                ArgumentNullException.ThrowIfNullOrEmpty(ImagePath);
                ImagePath.ThrowIfFileNotFound();

                _requestBody.Image = ImagePath;
                uploadFile = true;
                break;
            default:
                throw new ArgumentException($"Failed to build request body. Invalid OpenAI Task provided: {Mode}");
        }

        // Construct the HTTP request
        var apiRequest = new OpenAIRequest(
            endPoint: OpenAIEndpoint.Get(Mode),
            body: _requestBody,
            apiKeyPath: APIKeyPath,
            uploadFile: uploadFile
        );

        if (ShouldProcess(generateShouldProcessMsg(apiRequest), "Invoke OpenAI API for Image generation task"))
        {
            CancellationToken cancellationToken = new();

            var requestTask = apiRequest.InvokeAsync(cancellationToken);
            var taskResponse = requestTask.Result;
            if (requestTask.IsFaulted)
            {
                foreach (var exp in requestTask!.Exception!.InnerExceptions)
                {
                    WriteError(new(exp, "API Request Failure", ErrorCategory.InvalidOperation, _requestBody));
                }
                throw requestTask.Exception;
            }

            var responseContent = taskResponse?["content"];
            if (responseContent == null)
                WriteError(new(
                    new ArgumentNullException("Response content is null"),
                    "API Request Failure",
                    ErrorCategory.InvalidOperation,
                    _requestBody));

            WriteVerbose($"Quota usage: {responseContent!["usage"]}");

            var response = parseResponseContent(responseContent);
            _history.Add(new() { response });

            WriteObject(response);
        }
    }

    private string generateShouldProcessMsg(OpenAIRequest request)
    {
        float cost = 0;
        switch (Mode)
        {
            case OpenAITask.ImageGeneration:
            case OpenAITask.ImageEdit:
                cost = ApiCostEstimator.EstimateImageCost(request.Body.Size, request.Body.Prompt, request.Body.N);
                break;
            case OpenAITask.ImageVariation:
                cost = ApiCostEstimator.EstimateImageCost(request.Body.Size, String.Empty, request.Body.N);
                break;
            default:
                break;
        }
        return $@"
Sending a request to {request.EndPoint}
with API Key from: {request.APIKeyPath}    
estimated cost: {cost}
---
Request body : {request.Body}
---
@";
    }

    private OpenAIResponse parseResponseContent(JsonNode responseContent)
    {
        var query =
            from data in responseContent["data"]?.AsArray()
            select data["url"]?.ToString();

        return new OpenAIResponse()
        {
            Prompt = this.Prompt,
            Response = query.ToArray<string>()
        };
    }
}
