function Invoke-OpenAIRequest {
    [OutputType([OpenAIResponse])]
    [CmdletBinding(SupportsShouldProcess)]
    param (
        [Parameter(ParameterSetName = "Body")]
        [Parameter(ParameterSetName = "Form")]
        [Parameter(Mandatory, ValueFromPipeline, ValueFromPipelineByPropertyName)]
        [string] $Endpoint,

        [Parameter(Mandatory, ParameterSetName = "Body")]
        [hashtable] $Body,

        [Parameter(Mandatory, ParameterSetName = "Form")]
        [hashtable] $Form,

        [Parameter(ParameterSetName = "Body")]
        [Parameter(ParameterSetName = "Form")]
        [Parameter(Mandatory)]
        [securestring] $APIKey
    )
    
    $header = @{
        "Content-Type"  = "application/json";
        "Authorization" = "Bearer <Your API KEY>"
    }
    if ($PSCmdlet.ShouldProcess(@"
Uri: $uri
---
Header : $($header | ConvertTo-Json)
---
Body :  $($Body | ConvertTo-Json)
---
Form :  $($Form | ConvertTo-Json)
---
"@, "Invoke OpenAI API")) {
        if (-not $APIKey) {
            $APIKey = Get-OpenAIAPIKey 
            if ($null -eq $APIKey) {
                return
            }
        }
        $header["Authorization"] = "Bearer $(ConvertFrom-SecureString -SecureString $APIKey -AsPlainText)"
        try {
            if ($PSCmdlet.ParameterSetName -eq "Form") {
                $response = Invoke-WebRequest -Uri:$Endpoint -Method:Post -Headers:$header -Form:$form
            }
            else {
                $payload = $body | ConvertTo-Json
                $response = Invoke-WebRequest -Uri:$uri -Method:Post -Headers:$header -Body:$payload
            }
            return $response
        }
        catch {
            $StatusCode = $_.Exception.Response.StatusCode
            $ErrorMsg = $_.ErrorDetails.Message
            Write-Error "Request failed with code: $StatusCode `n$($ErrorMsg)"
            return
        }
    }
}

function Invoke-OpenAIText {
    [OutputType([OpenAIResponse])]
    [CmdletBinding(DefaultParameterSetName = "Default", SupportsShouldProcess)]
    [Alias("openai")]
    param (
        [Parameter(
            Mandatory,
            ValueFromPipeline, ValueFromPipelineByPropertyName,
            HelpMessage = "The prompt(s) to generate completions for, encoded as a string")]
        [ValidateLength(0, 4096)]
        [string] $Prompt,

        [Parameter(HelpMessage = "Text completion mode.  Note:'Chat' performs similar to 'TextCompletion' at 10% the price.")]
        [ValidateSet("TextCompletion", "ChatGPT")]
        [string] $Mode = "ChatGPT",

        [Parameter(HelpMessage = "Path to a text file with extra context")]
        [string] $FilePath,

        [Parameter(HelpMessage = "The maximum number of tokens to generate in the completion.")]
        [ValidateRange(1, 4096)]
        [int] $MaxTokens = 200,

        [Parameter(HelpMessage = "What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.")]
        [ValidateRange(0.0, 2.0)]
        [float] $Temperature = 0,

        [Parameter(HelpMessage = "An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are considered.")]
        [ValidateRange(0.0, 1.0)]
        [float] $Top_P = 1.0,

        [Parameter(HelpMessage = "This instruction sets the initial setting of the chat model")]
        [ValidateNotNullOrEmpty]
        [string] $ChatInitInstruction = "You are a helpful assistant",

        [Parameter(HelpMessage = "Up to 4 sequences where the API will stop generating further tokens. The returned text will not contain the stop sequence.")]
        [ValidateCount(0, 4)]
        [string[]] $StopSequences,

        [Parameter(HelpMessage = "Continue on the last conversation")]
        [Switch] $ContinueLastConversation,

        [Parameter(HelpMessage = "[SecureString] OpenAI API key")]
        [SecureString] $APIKey
    )

    $uri = "https://api.openai.com/v1/"
    $body = @{
        max_tokens  = $MaxTokens
        temperature = $Temperature
        top_p       = $Top_P
        n           = $Samples
    }
    if ($StopSequences) {
        $body["stop"] = $StopSequences
    }
    switch ($Mode) {
        "TextCompletion" { 
            $uri += "completions"
            $body["prompt"] = $Prompt
            $body["model"] = "text-davinci-003"
        }
        "ChatGPT" { 
            $uri += "chat/completions"
            $body["model"] = "gpt-3.5-turbo"
            if ($ContinueLastConversation -and $Global:LastChatGPTConversation) {
                $body["messages"] = $Global:LastChatGPTConversation + @{
                    role    = "user"
                    content = $Prompt
                }
            }
            else {
                $body["messages"] = @(
                    @{role = "system"; content = $ChatInitInstruction },
                    @{role = "user"; content = $Prompt }
                )
            }
        }
        default {
            Write-Error "Mode: $Mode is not valid!"
            return
        }
    }
    if (-ne $APIKey) {
        $APIKey = Get-OpenAIAPIKey
    }

    $response = Invoke-OpenAIRequest -Endpoint:$uri -Body:$body -APIKey:$APIKey
    $response = $response.Content | ConvertFrom-Json
    Write-Verbose $response | Select-Object -ExpandProperty Usage

    if ($ContinueLastConversation) {
        $Global:LastChatGPTConversation += $body["messages"][-1]
        $Global:LastChatGPTConversation += $response.choices | ForEach-Object {
            @{role = $_.Message.role; content = $_.Message.content }
        }
    }
    else {
        if ($Mode -eq "ChatGPT") {
            $Global:LastChatGPTConversation = $body["messages"] + @($response.choices | ForEach-Object {
                    @{role = $_.Message.role; content = $_.Message.content }
                })
        }
    }

    $openAIResponse = [OpenAIResponse]::new()
    $openAIResponse.Prompt = $Prompt
    $openAIResponse.Response = $response.choices | ForEach-Object {
        if ($_ | Get-Member "Message") {
            $_.Message.Content.Trim()
        }
        elseif ($_ | Get-Member "Text") {
            $_.Text.Trim()
        }
    }
    return $openAIResponse
}
class OpenAIResponse {
    [string] $Prompt; 
    [string[]] $Response;
}