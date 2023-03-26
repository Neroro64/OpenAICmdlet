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

<#
.SYNOPSIS
Fetches the file stored under your organization on OpenAI

.PARAMETER FileID
ID of the file to be fetched
#>
function Get-OpenAIFile {
    param(
        [string] $FileID
    )

    $apiKey = Get-OpenAIAPIKey
    if (-not $apiKey) {
        return
    }
    $uri = "https://api.openai.com/v1/files/$FileID"
    $header = @{
        Authorization = "Bearer $($apiKey | ConvertFrom-SecureString -AsPlainText)" 
    }
    return Invoke-WebRequest -Uri:$uri -Method:Get -Headers:$header 
}
<#
.SYNOPSIS
A basic function for building an OpenAI API request


.PARAMETER Endpoint
Tne OpenAI API endpoint to interact. 

.PARAMETER Body
The request body

.PARAMETER Form
The request form

.PARAMETER APIKey
Your OpenAI API Key as SecureString

#>
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
$uri with
---
Header : $($header | ConvertTo-Json)
---
Body :  $($Body | ConvertTo-Json)
---
Form :  $($Form | ConvertTo-Json)
---
"@, "Invoke OpenAI API Request")) {
        if (-not $APIKey) {
            $APIKey = Get-OpenAIAPIKey 
            if ($null -eq $APIKey) {
                return
            }
        }
        $header["Authorization"] = "Bearer $(ConvertFrom-SecureString -SecureString $APIKey -AsPlainText)"
        try {
            if ($PSCmdlet.ParameterSetName -eq "Form") {
                $response = Invoke-WebRequest -Uri:$Endpoint -Method:Post -Headers:$header -Form:$form -WhatIf
            }
            else {
                $payload = $body | ConvertTo-Json
                $response = Invoke-WebRequest -Uri:$EndPoint -Method:Post -Headers:$header -Body:$payload -Whatif
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

<#
.SYNOPSIS
Short description

.DESCRIPTION
Long description

.PARAMETER Prompt
Parameter description

.PARAMETER Mode
Parameter description

.PARAMETER FilePath
Parameter description

.PARAMETER MaxTokens
Parameter description

.PARAMETER Temperature
Parameter description

.PARAMETER Top_P
Parameter description

.PARAMETER ChatInitInstruction
Parameter description

.PARAMETER StopSequences
Parameter description

.PARAMETER ContinueLastConversation
Parameter description

.PARAMETER Samples
Parameter description

.PARAMETER APIKey
Parameter description

.EXAMPLE
An example

.NOTES
General notes
#>
function Invoke-OpenAIText {
    [OutputType([OpenAIResponse])]
    [CmdletBinding(DefaultParameterSetName = "Default", SupportsShouldProcess)]
    [Alias("ai")]
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
        [ValidateNotNullOrEmpty()]
        [string] $ChatInitInstruction = "You are a helpful assistant",

        [Parameter(HelpMessage = "Up to 4 sequences where the API will stop generating further tokens. The returned text will not contain the stop sequence.")]
        [ValidateCount(0, 4)]
        [string[]] $StopSequences,

        [Parameter(HelpMessage = "Continue on the last conversation")]
        [Switch] $ContinueLastConversation,

        [Parameter(HelpMessage = "Number of images that should be generated")]
        [int] $Samples = 1,

        [Parameter(HelpMessage = "[SecureString] OpenAI API key")]
        [SecureString] $APIKey
    )

    if (-not $APIKey) {
        $APIKey = Get-OpenAIAPIKey
        if ($null -eq $APIKey) {
            return
        }
    }

    $uri = "https://api.openai.com/v1/"
    $body = @{
        max_tokens  = $MaxTokens
        temperature = $Temperature
        top_p       = $Top_P
        n           = $Samples
    }

    if ($FilePath) {
        if (-not (Test-Path $FilePath)) {
            Write-Error "File: $FilePath not found!"
            return 
        }
        $Prompt = $Prompt + "`n$(Get-Content -Raw $FilePath)"
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

    $tokenCost = $Prompt.Split().Count + $Samples * $MaxTokens 
    $cost = $tokenCost * ($Mode -eq "ChatGPT")? 0.002 / 1000 : 0.02 / 1000
    if ($Global:LastChatGPTConversation) {
        $Global:LastChatGPTConversation | ForEach-Object { $_.Content.Split().Count } | Measure-Object -Sum
    }
    if ($PSCmdlet.ShouldProcess("Text completion with maximum estimated tokens: $tokenCost => `$$([math]::Round($cost, 6))", "Invoke OpenAI API")) {
        $response = Invoke-OpenAIRequest -Endpoint:$uri -Body:$body -APIKey:$APIKey
        if ($null -eq $response) {
            return
        }

        $response = $response.Content | ConvertFrom-Json
        Write-Verbose $response | Select-Object -ExpandProperty Usage

        if ($ContinueLastConversation) {
            [void]$Global:LastChatGPTConversation.Add($body["messages"][-1])
            $response.choices | ForEach-Object {
                [void]$Global:LastChatGPTConversation.Add(@{role = $_.Message.role; content = $_.Message.content })
            }
        }
        else {
            if ($Mode -eq "ChatGPT") {
                $Global:LastChatGPTConversation = [System.Collections.ArrayList]::new(@($body["messages"]) + @($response.choices | ForEach-Object {
                            @{role = $_.Message.role; content = $_.Message.content }
                        }))
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
        if (-not $Global:OpenAIResponses) {
            $Global:OpenAIResponses = [System.Collections.ArrayList]::new(@($openAIResponse))
        }
        else {
            [void]$Global:OpenAIResponses.Add($openAIResponse)
        }
        return $openAIResponse
    }
    else {
        Invoke-OpenAIRequest -Endpoint:$uri -Body:$body -APIKey:$APIKey -WhatIf 
    }
    return
}

<#
.SYNOPSIS
Short description

.DESCRIPTION
Long description

.PARAMETER Prompt
Parameter description

.PARAMETER Mode
Parameter description

.PARAMETER ImagePath
Parameter description

.PARAMETER ImageSize
Parameter description

.PARAMETER ImageMaskPath
Parameter description

.PARAMETER Samples
Parameter description

.PARAMETER APIKey
Parameter description

.EXAMPLE
An example

.NOTES
General notes
#>
function Invoke-OpenAIImage {
    [OutputType([OpenAIResponse])]
    [CmdletBinding(DefaultParameterSetName = "Default", SupportsShouldProcess)]
    [Alias("aiimg")]
    param (
        [Parameter(
            ValueFromPipeline, ValueFromPipelineByPropertyName,
            HelpMessage = "The prompt(s) to generate images or guide image edits")]
        [ValidateLength(0, 4096)]
        [string] $Prompt,

        [Parameter(HelpMessage = "Image generation mode")]
        [ValidateSet("Generation", "Edit", "Variation")]
        [string] $Mode = "Generation",

        [Parameter(HelpMessage = "Path to the input image file")]
        [ValidatePattern(".\.png$")]
        [string] $ImagePath,

        [Parameter(HelpMessage = "The size of the generated image")]
        [ValidateSet("256x256", "512x512", "1024x1024")]
        [string] $ImageSize = "256x256",

        [Parameter(HelpMessage = "Path to the image mask file")]
        [ValidatePattern(".\.png$")]
        [string] $ImageMaskPath,

        [Parameter(HelpMessage = "Number of images that should be generated")]
        [int] $Samples = 1,

        [Parameter(HelpMessage = "[SecureString] OpenAI API key")]
        [SecureString] $APIKey
    )

    if (-not $APIKey) {
        $APIKey = Get-OpenAIAPIKey
        if ($null -eq $APIKey) {
            return
        }
    }

    $uri = "https://api.openai.com/v1/"
    $body = @{ n = $Samples }

    switch ($Mode) {
        "Generation" {
            $uri += "images/generations"
            $body["prompt"] = $Prompt
            $body["size"] = $ImageSize
        }
        "Edit" {
            if (-not (Test-Path $ImagePath) -or -not (Test-Path $ImageMaskPath)) {
                Write-Error @"
Images not found!
For this task, the following image files must be provided:
-ImagePath: $ImagePath,
-ImageMaskPath: $ImageMaskPath
"@
                return 
            }
            $uri += "images/edits"
            $form = @{
                image  = Get-Item -Path $ImagePath
                mask   = Get-Item -Path $ImageMaskPath
                size   = $ImageSize
                prompt = $Prompt
                n      = $Samples
            }
        }
        "Variation" {
            if (-not (Test-Path $ImagePath)) {
                Write-Error @"
Images not found!
For this task, the following image file must be provided:
-ImagePath: $ImagePath,
"@
                return 
            }
            $uri += "images/variations"
            $form = @{
                size  = $ImageSize
                image = Get-Item -Path $ImagePath
                n     = $Samples
            }
        }
        default {
            Write-Error "Mode: $Mode is not valid!"
            return
        }
    }

    $imageGenerationCostModel = @{
        "256x256"   = 0.016
        "512x512"   = 0.018
        "1024x1024" = 0.02
    }
    $cost = $Samples * $imageGenerationCostModel[$ImageSize]
    if ($PSCmdlet.ShouldProcess("Image $Mode with estimated cost: `$$cost", "Invoke OpenAI API")) {
        if ($Mode -eq "Generation") {
            $payload = $body | ConvertTo-Json
            $response = Invoke-OpenAIRequest -Endpoint:$uri -Body:$payload -APIKey:$APIKey
        }
        else {
            $response = Invoke-OpenAIRequest -Endpoint:$uri -Form:$form -APIKey:$APIKey
        }
        if ($null -eq $response) {
            return
        }

        $response = $response.Content | ConvertFrom-Json
        Write-Verbose $response | Select-Object -ExpandProperty Usage

        $openAIResponse = [OpenAIResponse]::new()
        $openAIResponse.Prompt = $Prompt
        $openAIResponse.Response = $response.data.url

        if (-not $Global:OpenAIResponses) {
            $Global:OpenAIResponses = [System.Collections.ArrayList]::new(@($openAIResponse))
        }
        else {
            [void]$Global:OpenAIResponses.Add($openAIResponse)
        }
        return $openAIResponse
    }
    else {
        if ($Mode -eq "Generation") {
            $payload = $body | ConvertTo-Json
            $response = Invoke-OpenAIRequest -Endpoint:$uri -Body:$payload -APIKey:$APIKey -WhatIf
        }
        else {
            $response = Invoke-OpenAIRequest -Endpoint:$uri -Form:$form -APIKey:$APIKey -WhatIf
        }
    }
    return
}

<#
.SYNOPSIS
Short description

.DESCRIPTION
Long description

.PARAMETER Prompt
Parameter description

.PARAMETER Mode
Parameter description

.PARAMETER Temperature
Parameter description

.PARAMETER AudioPath
Parameter description

.PARAMETER AudioLanguage
Parameter description

.PARAMETER APIKey
Parameter description

.EXAMPLE
An example

.NOTES
General notes
#>
function Invoke-OpenAIAudio {
    [OutputType([OpenAIResponse])]
    [CmdletBinding(DefaultParameterSetName = "Default", SupportsShouldProcess)]
    [Alias("aiaudio")]
    param (
        [Parameter(
            ValueFromPipeline, ValueFromPipelineByPropertyName,
            HelpMessage = "The prompt(s) to guide the mode's style or continue on previous audio segment")]
        [ValidateLength(0, 4096)]
        [string] $Prompt,

        [Parameter(HelpMessage = "Image generation mode")]
        [ValidateSet("Transcription", "Translation")]
        [string] $Mode = "Transcription",

        [Parameter(HelpMessage = "What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.")]
        [ValidateRange(0.0, 2.0)]
        [float] $Temperature = 0,

        [Parameter(HelpMessage = "Path to the input audio file")]
        [ValidatePattern("(.\.mp3$)|(.\.mp4$)|(.\.mpeg$)|(.\.wav$)|(.\.webm$)")]
        [string] $AudioPath,

        [Parameter(HelpMessage = "The language of the input audio. Supplying the input language in ISO-639-1 format will improve accuracy and latency.")]
        [string] $AudioLanguage,

        [Parameter(HelpMessage = "[SecureString] OpenAI API key")]
        [SecureString] $APIKey
    )

    if (-not $APIKey) {
        $APIKey = Get-OpenAIAPIKey
        if ($null -eq $APIKey) {
            return
        }
    }

    if (-not (Test-Path $AudioPath)) {
        Write-Error @"
Audio file not found!
For this task, the following audio file must be provided:
-AudioFile: $AudioPath,
"@
        return 
    }

    $uri = "https://api.openai.com/v1/"
    switch ($Mode) {
        "Transcription" {
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
            $uri += "audio/translations"
            $form = @{
                file        = Get-Item -Path $AudioPath
                model       = "whisper-1"
                prompt      = $Prompt ?? ""
                temperature = $Temperature
            }
        }
    }

    if ($PSCmdlet.ShouldProcess("Audio $Mode with the cost model: `$0.006 / minute", "Invoke OpenAI API")) {
        $response = Invoke-OpenAIRequest -Endpoint:$uri -Form:$form -APIKey:$APIKey
        if ($null -eq $response) {
            return
        }

        $response = $response.Content | ConvertFrom-Json
        Write-Verbose $response | Select-Object -ExpandProperty Usage

        $openAIResponse = [OpenAIResponse]::new()
        $openAIResponse.Prompt = $Prompt
        $openAIResponse.Response = $response.text

        if (-not $Global:OpenAIResponses) {
            $Global:OpenAIResponses = [System.Collections.ArrayList]::new(@($openAIResponse))
        }
        else {
            [void]$Global:OpenAIResponses.Add($openAIResponse)
        }
        return $openAIResponse
    }
    else {
        Invoke-OpenAIRequest -Endpoint:$uri -Form:$form -APIKey:$APIKey -WhatIf
    }
    return
}
class OpenAIResponse {
    [string] $Prompt; 
    [string[]] $Response;
}