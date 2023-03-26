<#
.SYNOPSIS
A simple PowerShell cmdlet for invoking OpenAI's API to perform the tasks descrined in https://platform.openai.com/docs/introduction

.DESCRIPTION
Long description

.PARAMETER Prompt
The prompt(s) to generate completions for, encoded as a string

.PARAMETER Task
Task to be performed by OpenAI's models

.PARAMETER MaxTokens
The maximum number of tokens to generate in the completion.

.PARAMETER Temperature
What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.

.PARAMETER ChatInitInstruction
For Chat task, this instruction sets the mood of the model

.PARAMETER StopSequences
Up to 4 sequences where the API will stop generating further tokens. The returned text will not contain the stop sequence.

.PARAMETER ContinueLastConversation
Continue on the last conversation

.PARAMETER ImageSize
The size of the generated image

.PARAMETER ImagePath
Path to the input image

.PARAMETER ImageMaskPath
Path to the image mask

.PARAMETER AudioPath
Path to the input audio file

.PARAMETER AudioLanguage
The language of the input audio. Supplying the input language in ISO-639-1 format will improve accuracy and latency.

.PARAMETER Samples
Number of responses that should be generated

.PARAMETER APIKey
Your OpenAI API key as SecureString.
For details, check https://openai.com/blog/openai-api


.EXAMPLE
Invoke-OpenAI -Task:"Text" -Prompt:"Say this is a test" -Temperature:0 -MaxTokens:7 -Samples:1 -StopSequences:"\n" 

.EXAMPLE
Invoke-OpenAI -Task:"Chat" -Prompt:"Hello!" -Temperature:0 -MaxTokens:7 -Samples:1 -StopSequences:"\n" 

.EXAMPLE
Invoke-OpenAI -Task:"Image" -Prompt:"A cute baby sea otter" -ImageSize:"512x512" -Samples:1

.EXAMPLE
Invoke-OpenAI -Task:"ImageEdit"; Prompt:"A cute baby sea otter wearing a beret"; ImagePath:"otter.png"; ImageMaskPath:"mask.png"; Samples:1; ImageSize:"512x512"

.EXAMPLE
Invoke-OpenAI -Task = "ImageVariation" -ImagePath = "otter.png" -ImageSize = "512x512" -Samples = 1

.EXAMPLE
Invoke-OpenAI -Task:"Transcription" -AudioPath:"audio.mp3" -AudioLanguage:"English" -Temperature:1

.EXAMPLE
Invoke-OpenAI -Task:"Translation"; AudioPath:"audio.mp3"; Temperature:1

.NOTES
Invoking OpenAPI API will consume your token quota or inflict costs. Use with caustion.

.NOTES
Remember not to upload prorietary information or property to OpenAI
#>
function Invoke-OpenAI {
    [OutputType([OpenAIResponse])]
    [CmdletBinding(SupportsShouldProcess)]
    [Alias("openai")]
    param(
        [Parameter(ParameterSetName = "Text")]
        [Parameter(ParameterSetName = "Image")]
        [Parameter(Position = 0, ValueFromPipeline, ValueFromPipelineByPropertyName,
            HelpMessage = "The prompt(s) to generate completions for, encoded as a string")]
        [ValidateLength(0, 4096)]
        [string] $Prompt = "",

        [Parameter(ParameterSetName = "Text")]
        [Parameter(ParameterSetName = "Image")]
        [Parameter(ParameterSetName = "Audio",
            HelpMessage = "Task to be performed by OpenAI's models. Note:'Chat' performs similar to 'TextCompletion' at 10% the price.")]
        [ValidateSet("TextCompletion", "Chat", "ImageGeneration", "ImageEdit", "ImageVariation", "Transcription", "Translation")]
        [string] $Task = "Chat",

        [Parameter(ParameterSetName = "Text",
            HelpMessage = "The maximum number of tokens to generate in the completion.")]
        [ValidateRange(1, 4096)]
        [int] $MaxTokens = 200,

        [Parameter(ParameterSetName = "Text")]
        [Parameter(ParameterSetName = "Audio",
            HelpMessage = "What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.")]
        [ValidateRange(0.0, 2.0)]
        [float] $Temperature = 0,

        [Parameter(ParameterSetName = "Text",
            HelpMessage = "For Chat task, this instruction sets the mood of the model")]
        [string] $ChatInitInstruction = "You are a helpful assistant",

        [Parameter(ParameterSetName = "Text", 
            HelpMessage = "Up to 4 sequences where the API will stop generating further tokens. The returned text will not contain the stop sequence.")]
        [ValidateCount(0, 4)]
        [string[]] $StopSequences,
        
        [Parameter(ParameterSetName = "Text",
            HelpMessage = "Continue on the last conversation")]
        [Switch] $ContinueLastConversation,

        [Parameter(ParameterSetName = "Image",
            HelpMessage = "The size of the generated image")]
        [ValidateSet("256x256", "512x512", "1024x1024")]
        [string] $ImageSize = "256x256",

        [Parameter(ParameterSetName = "Image",
            HelpMessage = "Path to the input image")]
        [ValidatePattern(".\.png$")]
        [string] $ImagePath,

        [Parameter(ParameterSetName = "Image",
            HelpMessage = "Path to the image mask")]
        [ValidatePattern(".\.png$")]
        [string] $ImageMaskPath,

        [Parameter(Mandatory, ParameterSetName = "Audio",
            HelpMessage = "Path to the input audio file")]
        [ValidatePattern("(.\.mp3$)|(.\.mp4$)|(.\.mpeg$)|(.\.wav$)|(.\.webm$)")]
        [string] $AudioPath,

        [Parameter(ParameterSetName = "Audio",
            HelpMessage = "The language of the input audio. Supplying the input language in ISO-639-1 format will improve accuracy and latency.")]
        [string] $AudioLanguage,

        [Parameter(HelpMessage = "Number of responses that should be generated")]
        [int] $Samples = 1,

        [Parameter(HelpMessage = "Your OpenAI API key as SecureString.")]
        [SecureString] $APIKey
    )

    if (-not $APIKey) {
        $APIKey = Get-OpenAIAPIKey 
        if ($null -eq $APIKey) {
            return
        }
    }

    $header = @{
        "Content-Type"  = "application/json";
        "Authorization" = "Bearer $(ConvertFrom-SecureString -SecureString $APIKey -AsPlainText)"
    }

    $body = @{}
    if ($PSCmdlet.ParameterSetName -ne "Audio") {
        $body["n"] = $Samples;
    }


    if (-not [string]::IsNullOrEmpty($EditInstruction)) { $Task = "Edit" }
    $uri = "https://api.openai.com/v1/"
    switch ($Task) {
        "TextCompletion" { 
            $uri += "completions"
            $body["prompt"] = $Prompt
            $body["model"] = "text-davinci-003"
            $body["max_tokens"] = $MaxTokens
            $body["temperature"] = $Temperature
            if ($StopSequences) {
                $body["stop"] = $StopSequences
            }
        }
        "Chat" { 
            $uri += "chat/completions"
            $body["model"] = "gpt-3.5-turbo"
            $body["max_tokens"] = $MaxTokens
            $body["temperature"] = $Temperature
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
            if ($StopSequences) {
                $body["stop"] = $StopSequences
            }
        }
        "ImageGeneration" {
            $uri += "images/generations"
            $body["prompt"] = $Prompt
            $body["size"] = $ImageSize
        }
        "ImageEdit" {
            if (-not (Test-Path $ImagePath) -or -not (Test-Path $ImageMaskPath)) {
                Write-Error @"
Images not found!
For task:$Task, you must provide the following image files:
ImagePath: $ImagePath,
ImageMaskPath: $ImageMaskPath
"@
                return 
            }
            $uri += "images/edits"
            $form = @{
                image  = Get-Item -Path $ImagePath
                mask   = Get-Item -Path $ImageMaskPath
                size   = $ImageSize
                prompt = $Prompt
            }
        }
        "ImageVariation" {
            if (-not (Test-Path $ImagePath)) {
                Write-Error @"
Images not found!
For task:$Task, you must provide the following image files:
ImagePath: $ImagePath,
"@
                return 
            }
            $uri += "images/variations"
            $form = @{
                size  = $ImageSize
                image = Get-Item -Path $ImagePath
            }
        }
        "Transcription" {
            if (-not (Test-Path $AudioPath)) {
                Write-Error @"
Audio not found!
For task:$Task, you must provide the following audio files:
AudioFile: $AudioPath,
"@
                return 
            }
            $uri += "audio/transcriptions"
            $form = @{
                file        = Get-Item -Path $AudioPath
                model       = "whisper-1"
                prompt      = $Prompt ?? ""
                temperature = $Temperature
            }
            if ($Language) {
                $form["language"] = $Language
            }
        }
        "Translation" {
            if (-not (Test-Path $AudioPath)) {
                Write-Error @"
Audio not found!
For task:$Task, you must provide the following audio files:
AudioFile: $AudioPath,
"@
                return 
            }
            $uri += "audio/translations"
            $form = @{
                file        = Get-Item -Path $AudioPath
                model       = "whisper-1"
                prompt      = $Prompt ?? ""
                temperature = $Temperature
            }
        }
    }
    if ($PSCmdlet.ShouldProcess(`
                @"
Estimated cost (only applicable for text completion): $($Prompt.Split().Count + $Samples * $MaxTokens)
---
Uri: $uri
---
Header : $($header | ConvertTo-Json)
---
Body :  $($body | ConvertTo-Json)
---
Form :  $($form | ConvertTo-Json)
---
"@, "Invoke OpenAI API")) {
        try {
            if ($form) {
                $response = Invoke-WebRequest -Uri:$uri -Method:Post -Headers:$header -Form $form
                return $response
            }
            else {
                $payload = $body | ConvertTo-Json
                $response = Invoke-WebRequest -Uri:$uri -Method:Post -Headers:$header -Body:$payload
                return response
                # $response = Get-Content $PSScriptRoot/Tests/MockChatResponse.json | ConvertFrom-Json
            }
        }
        catch {
            $StatusCode = $_.Exception.Response.StatusCode
            $ErrorMsg = $_.ErrorDetails.Message
            Write-Error "Request failed with code $StatusCode `n$($ErrorMsg)"
            return
        }
        $response = $response.Content | ConvertFrom-Json
        Write-Verbose $response | Select-Object -ExpandProperty Usage

        if ($ContinueLastConversation) {
            $Global:LastChatGPTConversation += $body["messages"][-1]
            $Global:LastChatGPTConversation += $response.choices | ForEach-Object {
                @{role = $_.Message.role; content = $_.Message.content }
            }
        }
        else {
            if ($Task -eq "Chat") {
                $Global:LastChatGPTConversation = $body["messages"] + @($response.choices | ForEach-Object {
                        @{role = $_.Message.role; content = $_.Message.content }
                    })
            }
        }

        $openAIResponse = [OpenAIResponse]::new()
        $openAIResponse.Prompt = $Prompt
        switch ($PSCmdlet.ParameterSetName) {
            "Text" {
                $openAIResponse.Response = $response.choices | ForEach-Object {
                    if ($_ | Get-Member "Message") {
                        $_.Message.Content.Trim()
                    }
                    elseif ($_ | Get-Member "Text") {
                        $_.Text.Trim()
                    }
                }
            }
            "Image" {
                $openAIResponse.Response = $response.data.url
            }
            "Audio" {
                $openAIResponse.Response = $response.text
            }
        }

        $Global:LastOpenAIResponse = $openAIResponse
        return $openAIResponse
    }
}

<#
.SYNOPSIS
Stores your OpenAI API key as SecureString to the path defined by $ENV:OPENAI_API_KEY
(default: $PSScriptRoot/OpenAI_API.key)

.PARAMETER FilePath
The file path where the API Key will be stored in
#>
function Set-OpenAIAPIKey {
    param (
        [string] $FilePath = $ENV:OPENAI_API_KEY ?? "$PSScriptRoot/OpenAI_API.key"
    )
    $EncryptionKey = [int[]] (Get-Random -SetSeed 2048 -Maximum ([byte]::MaxValue) -Count 16)
    $apiKey = Read-Host -Prompt "Enter your API key: " -AsSecureString 
    $apiKey = ConvertFrom-SecureString -SecureString $apiKey -Key $EncryptionKey
    $apiKey | Set-Content $FilePath
    Write-Host "Success! The API key is encrypted and stored in $FilePath" -ForegroundColor Green
}

<#
.SYNOPSIS
Loads your OpenAI API key as SecureString from the path defined by $ENV:OPENAI_API_KEY
(default: $PSScriptRoot/OpenAI_API.key)

.PARAMETER FilePath
The file path to the API Key
#>
function Get-OpenAIAPIKey {
    [OutputType([SecureString])]
    param (
        [string] $FilePath = $ENV:OPENAI_API_KEY ?? "$PSScriptRoot/OpenAI_API.key"
    )
    if (-not (Test-Path $FilePath)) {
        Write-Error "API Key not found: $FilePath `nPlease use Set-OpenAPIKey"
        return $null
    }
    $DecryptionKey = [int[]] (Get-Random -SetSeed 2048 -Maximum ([byte]::MaxValue) -Count 16)
    $key = Get-Content $FilePath | ConvertTo-SecureString -Key $DecryptionKey
    return $key
}
function Get-OpenAIFile {
    param(
        [uri] $file_id
    )

    $apiKey = Get-OpenAIAPIKey
    $uri = "https://api.openai.com/v1/files/$file_id"
    $header = @{
        Authorization = "Bearer $($apiKey | ConvertFrom-SecureString -AsPlainText)" 
    }
    return Invoke-WebRequest -Uri:$uri -Method:Get -Headers:$header 
}
class OpenAIResponse {
    [string] $Prompt; 
    [string[]] $Response;
}
