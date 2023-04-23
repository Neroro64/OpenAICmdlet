namespace OpenAICmdlet;

[Cmdlet(VerbsLifecycle.Invoke, "OpenAIText", SupportsShouldProcess = true)]
[Alias("gpt")]
[OutputType(typeof(OpenAIResponse))]
public class InvokeOpenAITextCommand : MyCmdlet
{
    private static List<OpenAIResponse> _history = new();

    [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true,
    HelpMessage = "The prompt(s) to generate completions for, encoded as a string")]
    [ValidateLength(0, 4096)]
    public string Prompt { get; set; } = "";

    [Parameter(HelpMessage = "Text completion mode.  Note:'ChatGPT' performs similar to 'TextCompletion' at 10% the price.")]
    [ValidateSet(nameof(OpenAITask.ChatCompletion), nameof(OpenAITask.TextCompletion))]
    public OpenAITask Mode { get; set; } = OpenAITask.TextCompletion;

    [Parameter(HelpMessage = "Path to a text file with extra context", ValueFromPipelineByPropertyName = true)]
    public string? ContextFilePath { get; set; }

    [Parameter(HelpMessage = "The suffix that comes after a completion of inserted text.")]
    public string? Suffix { get; set; }

    [Parameter(HelpMessage = "The maximum number of tokens to generate in the completion.")]
    [ValidateRange(1, 4096)]
    public int MaxTokens { get; set; } = Constant.defaultRequestParam.Max_Tokens;

    [Parameter(HelpMessage = "What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.")]
    [ValidateRange(0.0, 2.0)]
    public float Temperature { get; set; } = Constant.defaultRequestParam.Temperature;

    [Parameter(HelpMessage = "An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are considered.")]
    [ValidateRange(0.0, 1.0)]
    public float Top_P { get; set; } = Constant.defaultRequestParam.Top_p;

    [Parameter(HelpMessage = "Positive values penalize new tokens based on whether they appear in the text so far, increasing the model's likelihood to talk about new topics.")]
    [ValidateRange(-2.0, 2.0)]
    public float PrecencePenalty { get; set; } = 0;

    [Parameter(HelpMessage = "Positive values penalize new tokens based on their existing frequency in the text so far, decreasing the model's likelihood to repeat the same line verbatim.")]
    [ValidateRange(-2.0, 2.0)]
    public float FrequencyPenalty { get; set; } = 0;

    [Parameter(HelpMessage = "This instruction sets the initial setting of the chat model")]
    [ValidateNotNullOrEmpty()]
    public string ChatInitInstruction { get; set; } = "You are a helpful assistant";

    [Parameter(HelpMessage = "Up to 4 sequences where the API will stop generating further tokens. The returned text will not contain the stop sequence.")]
    [ValidateCount(0, 4)]
    public string[]? StopSequences { get; set; }

    [Parameter(HelpMessage = "Continue on the last conversation")]
    public SwitchParameter ContinueLastConversation { get; set; }

    [Parameter(HelpMessage = "Number of images that should be generated")]
    public int Samples { get; set; } = Constant.defaultRequestParam.N;

    [Parameter(HelpMessage = "Path to OpenAI API key file")]
    public string? APIKeyPath { get; set; }

    private OpenAIRequestBody _requestBody = new();
    protected override void BeginProcessing()
    {
        _requestBody = new()
        {
            Frequency_penalty = FrequencyPenalty,
            Max_Tokens = MaxTokens,
            Model = OpenAIModel.TaskModel(Mode),
            N = Samples,
            Precence_penalty = PrecencePenalty,
            Stop = StopSequences,
            Suffix = Suffix,
            Temperature = Temperature,
            Top_p = Top_P,
        };
    }
    protected override void ProcessRecord()
    {
        switch (Mode)
        {
            case OpenAITask.TextCompletion:
                _requestBody.Prompt = PromptBuilder.BuildPrompt(Prompt, ContextFilePath);
                break;
            case OpenAITask.ChatCompletion:
                _requestBody.Messages = PromptBuilder.BuildChat(ChatInitInstruction, Prompt, ContextFilePath, ContinueLastConversation, _history);
                break;
            default:
                throw new ArgumentException($"Failed to build prompt. Invalid OpenAI Task provided: {Mode}");
        }

        var apiRequest = new OpenAIRequest(
            endPoint: OpenAIEndpoint.Get(Mode),
            body: _requestBody,
            apiKeyPath: APIKeyPath
        );

        if (ShouldProcess(generateShouldProcessMsg(apiRequest), "Invoke OpenAI API for text complection task"))
        {
            CancellationToken cancellationToken = new();

            var requestTask = apiRequest.InvokeAsync(cancellationToken);
            var taskResponse = requestTask.Result;
            if (requestTask.IsFaulted)
            {
                foreach (var exp in requestTask.Exception.InnerExceptions)
                {
                    WriteError(new(exp, "API Request Failure", ErrorCategory.InvalidOperation, _requestBody));
                }
                throw requestTask.Exception;
            }

            var responseContent = taskResponse?["content"];
            if (responseContent == null)
                WriteError(new(new ArgumentNullException("Response content is null"), "API Request Failure", ErrorCategory.InvalidOperation, _requestBody));

            WriteVerbose($"Quota usage: {responseContent?["usage"]}");
            var response = new OpenAIResponse()
            {
                Prompt = this.Prompt,
                Response = responseContent?["text"]?.ToString() ?? ""
            };
            if (ContinueLastConversation && Mode == OpenAITask.ChatCompletion)
            {
                _history.Add(response);
            }
            else
                _history = new() { response };

            WriteObject(response);
        }
    }

    private string generateShouldProcessMsg(OpenAIRequest request)
    {
        float cost = 0;
        if (Mode == OpenAITask.TextCompletion)
            cost = ApiCostEstimator.EstimateTokenCost(request.Body.Prompt, request.Body.Model);
        else
        {
            var messagesAsSingleStr = String.Join("\n", request.Body?.Messages?.Select(x => x["content"]).ToList() ?? new());
            cost = ApiCostEstimator.EstimateTokenCost(messagesAsSingleStr, request.Body?.Model);
        }
        return $@"
Sending a request to {request.EndPoint}
with API Key from: {request.APIKeyPath}    
estimated cost: {cost}
---
Request body : {_requestBody}
---
@";
    }
}

//string shouldProcessDesc = $@"
//if (ShouldProcess(shouldProcessDesc; shouldProcessDesc, "Invoke OpenAI API request"))
//{