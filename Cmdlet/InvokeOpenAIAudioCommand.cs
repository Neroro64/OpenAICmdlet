namespace OpenAICmdlet;

[Cmdlet(VerbsLifecycle.Invoke, "OpenAIAudio", SupportsShouldProcess = true)]
[Alias("whisper")]
[OutputType(typeof(OpenAIResponse))]
public class InvokeOpenAIAudioCommand : MyCmdlet
{
    private static List<List<OpenAIResponse>> _history = new();

    [Parameter(
        Mandatory = true,
        Position = 0,
        ValueFromPipeline = true, ValueFromPipelineByPropertyName = true,
        HelpMessage = "Path to the input audio file")]
    [ValidatePattern("(.mp3$)|(.mp4$)|(.mpeg$)|(.wav$)|(.webm$)")]
    public string AudioPath { get; set; } = String.Empty;

    [Parameter(HelpMessage = "Speect-to-text task")]
    [ValidateSet(nameof(OpenAITask.AudioTranscription), nameof(OpenAITask.AudioTranslation))]
    public OpenAITask Mode { get; set; } = OpenAITask.AudioTranscription;

    [Parameter(HelpMessage = "The prompt(s) to guide the mode's style or continue on previous audio segment")]
    [ValidateLength(0, 4096)]
    public string Prompt { get; set; } = String.Empty;

    [Parameter(HelpMessage = "The language of the input audio. Supplying the input language in ISO-639-1 format will improve accuracy and latency.")]
    public string AudioLanguage { get; set; } = String.Empty;

    [Parameter(HelpMessage = "What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.")]
    [ValidateRange(0.0, 2.0)]
    public float Temperature { get; set; } = Constant.defaultRequestParam.Temperature;

    [Parameter(HelpMessage = "Path to OpenAI API key file")]
    public string? APIKeyPath { get; set; }

    private OpenAIRequestBody _requestBody = new();
    protected override void BeginProcessing()
    {
        _requestBody = new()
        {
            Model = OpenAIModel.TaskModel(Mode),
            Temperature = Temperature,
            Language = AudioLanguage
        };
    }
    protected override void ProcessRecord()
    {
        AudioPath.ThrowIfFileNotFound();
        _requestBody.File = AudioPath;
        _requestBody.Prompt = Prompt;

        // Construct the HTTP request
        var apiRequest = new OpenAIRequest(
            endPoint: OpenAIEndpoint.Get(Mode),
            body: _requestBody,
            apiKeyPath: APIKeyPath,
            uploadFile: true
        );

        if (ShouldProcess(generateShouldProcessMsg(apiRequest), "Invoke OpenAI API for Speech to text task"))
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

    private static string generateShouldProcessMsg(OpenAIRequest request)
    {
        // TODO: Get audio length from FileInfo
        float cost = ApiCostEstimator.EstimateAudioCost(1, request.Body.Prompt, 1);
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
        return new OpenAIResponse()
        {
            Prompt = this.Prompt,
            Response = new string[1] { responseContent["text"]?.ToString() ?? String.Empty }
        };
    }
}
