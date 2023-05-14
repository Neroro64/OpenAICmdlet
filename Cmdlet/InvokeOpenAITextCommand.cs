namespace OpenAICmdlet;

[Cmdlet(VerbsLifecycle.Invoke, "OpenAIText", SupportsShouldProcess = true)]
[Alias("igpt")]
[OutputType(typeof(Response))]
public class InvokeOpenAITextCommand : MyOpenAICmdlet
{
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
               ValueFromPipelineByPropertyName = true,
               HelpMessage = "The prompt(s) to generate completions for, encoded as a string")]
    [ValidateLength(0, 4096)]
    public string Prompt { get; set; } = String.Empty;

    [Parameter(
        HelpMessage =
            "Text completion mode.  Note:'ChatGPT' performs similar to 'TextCompletion' at 10% the price.")]
    [ValidateSet(nameof(OpenAITask.ChatCompletion), nameof(OpenAITask.TextCompletion))]
    public OpenAITask Mode
    {
        get; set;
    } = OpenAITask.ChatCompletion;

    [Parameter(HelpMessage = "Path to a text file with extra context",
               ValueFromPipelineByPropertyName = true)]
    public string? ContextFilePath
    {
        get; set;
    }

    [Parameter(HelpMessage = "The suffix that comes after a completion of inserted text.")]
    public string? Suffix
    {
        get; set;
    }

    [Parameter(HelpMessage = "The maximum number of tokens to generate in the completion.")]
    [ValidateRange(1, 4096)]
    public int MaxTokens
    {
        get; set;
    } = Constant.defaultRequestParam.Max_Tokens;

    [Parameter(
        HelpMessage =
            "What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.")]
    [ValidateRange(0.0, 2.0)]
    public float Temperature
    {
        get; set;
    } = Constant.defaultRequestParam.Temperature;

    [Parameter(
        HelpMessage =
            "An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are considered.")]
    [ValidateRange(0.0, 1.0)]
    public float Top_p
    {
        get; set;
    } = Constant.defaultRequestParam.Top_p;

    [Parameter(
        HelpMessage =
            "Positive values penalize new tokens based on whether they appear in the text so far, increasing the model's likelihood to talk about new topics.")]
    [ValidateRange(-2.0, 2.0)]
    public float PrecencePenalty
    {
        get; set;
    } = 0;

    [Parameter(
        HelpMessage =
            "Positive values penalize new tokens based on their existing frequency in the text so far, decreasing the model's likelihood to repeat the same line verbatim.")]
    [ValidateRange(-2.0, 2.0)]
    public float FrequencyPenalty
    {
        get; set;
    } = 0;

    [Parameter(HelpMessage = "This instruction sets the initial setting of the chat model")]
    [ValidateNotNullOrEmpty()]
    public string ChatInitInstruction
    {
        get; set;
    } = "You are a helpful assistant";

    [Parameter(
        HelpMessage =
            "Up to 4 sequences where the API will stop generating further tokens. The returned text will not contain the stop sequence.")]
    [ValidateCount(0, 4)]
    public IEnumerable<string>? StopSequences
    {
        get; set;
    }

    [Parameter(HelpMessage = "Continue on a previous session")]
    public SwitchParameter ContinueSession
    {
        get; set;
    }

    [Parameter(HelpMessage = "Species which session to continue")]
    public int SessionID
    {
        get; set;
    } = -1;

    [Parameter(HelpMessage = "Number of images that should be generated")]
    public int Samples
    {
        get; set;
    } = Constant.defaultRequestParam.N;

    [Parameter(HelpMessage = "Path to OpenAI API key file")]
    public string? APIKeyPath
    {
        get; set;
    }

    private RequestBody _requestBody = new();
    protected override void BeginProcessing()
    {
        _requestBody = new()
        {
            Frequency_penalty = FrequencyPenalty,
            Max_Tokens = MaxTokens,
            Model = Model.TaskModel(Mode),
            N = Samples,
            Precence_penalty = PrecencePenalty,
            Stop = StopSequences,
            Suffix = Suffix,
            Temperature = Temperature,
            Top_p = Top_p,
        };
    }
    protected override void ProcessRecord()
    {
        // Build the prompt
        List<Response>? sessionToContinue = default;
        if (ContinueSession)
        {
            if (SessionID >= History.Count)
                WriteError(new ErrorRecord(
                    new ArgumentException(
                        "Invalid ContinueOnSessionID! It is greater that the number of sessions"),
                    "Invalid ContinueOnSessionID", ErrorCategory.InvalidArgument, this));

            else if (SessionID == -1)
                sessionToContinue = History.LastOrDefault(new List<Response>());
            else
                sessionToContinue = History[SessionID];
        }

        switch (Mode)
        {
            case OpenAITask.TextCompletion:
                _requestBody.Prompt = PromptBuilder.BuildPrompt(Prompt, ContextFilePath);
                break;
            case OpenAITask.ChatCompletion:
                _requestBody.Messages = PromptBuilder.BuildChat(ChatInitInstruction, Prompt,
                                                                ContextFilePath, sessionToContinue);
                break;
            default:
                throw new ArgumentException(
                    $"Failed to build prompt. Invalid OpenAI Task provided: {Mode}");
        }

        // Construct the HTTP request
        var apiRequest = new Request(endPoint: Endpoint.Get(Mode), body: _requestBody,
                                           apiKeyPath: APIKeyPath);

        if (ShouldProcess(generateShouldProcessMsg(apiRequest),
                          "Invoke OpenAI API for text complection task"))
        {
            CancellationToken cancellationToken = new();

            var requestTask = apiRequest.InvokeAsync(cancellationToken);
            var responseContent = requestTask.Result;
            if (requestTask.IsFaulted)
            {
                if (requestTask.Exception != null)
                {
                    foreach (var exp in requestTask.Exception.InnerExceptions)
                    {
                        WriteError(new(exp, "API Request Failure", ErrorCategory.InvalidOperation,
                                       _requestBody));
                    }
                    throw requestTask.Exception;
                }
                else
                    WriteError(new(new OperationCanceledException("API Request Failure"),
                                   "API Request Failure", ErrorCategory.InvalidOperation,
                                   _requestBody));
            }

            if (responseContent == null)
                WriteError(new(new RuntimeException("Response content is null"),
                               "API Request Failure", ErrorCategory.InvalidOperation,
                               _requestBody));

            WriteVerbose($"Quota usage: {responseContent!["usage"]}");

            var response = parseResponseContent(responseContent, Mode);
            if (ContinueSession && History.Count > 0)
            {
                if (SessionID == -1)
                    History[^1].Add(response);
                else
                    History[SessionID].Add(response);
            }
            else
                History.Add(new() { response });

            WriteObject(response);
        }
    }

    private string generateShouldProcessMsg(Request request)
    {
        float cost = 0;
        if (Mode == OpenAITask.TextCompletion)
            cost = ApiCostEstimator.EstimateTokenCost(request.Body.Prompt, request.Body.Model,
                                                      request.Body.N);
        else
        {
            var messagesAsSingleStr = String.Join(
                "\n", request.Body.Messages?.Select(x => x["content"]).ToList() ?? new());
            cost = ApiCostEstimator.EstimateTokenCost(messagesAsSingleStr, request.Body.Model,
                                                      request.Body?.N ?? 1);
        }
        return $@"
Sending a request to {request.EndPoint}
with API Key from: {request.APIKeyPath}    
estimated cost: {cost}
---
Request body : {JsonSerializer.Serialize(request.Body, options: Constant.SerializerOption)}
---
";
    }
    private Response parseResponseContent(JsonNode responseContent, OpenAITask mode)
    {
        var query = mode switch
        {
            OpenAITask.ChatCompletion =>
                from choice in responseContent["choices"]?.AsArray()
                let message =
                    choice["message"]?.AsObject()
                select message["content"]?.ToString(),
            OpenAITask.TextCompletion or
                _ => from choice in responseContent["choices"]
                         ?.AsArray()
                     select choice["text"]
                         ?.ToString()
        };

        return new Response() { Prompt = this.Prompt, Body = query.ToArray<string>() };
    }
}
