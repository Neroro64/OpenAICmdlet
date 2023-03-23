function Invoke-OpenAIText {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [Parameter(Mandatory, ValueFromPipeline)]
        [string] $Prompt,

        [ValidateSet("Completion", "Chat", "Edit")]
        [string] $Task = "Completion",

        [ValidateRange(1, 4096)]
        [int] $MaxTokens = 200,

        [ValidateRange(0.0, 2.0)]
        [float] $Temperature = 0,

        [int] $Samples = 1,

        [ValidateCount(0,4)]
        [string[]] $StopSequences,
        
        [string] $EditInstruction,
        [Switch] $ContinueLastConversation,
        [string] $APIKey
    )

    if (-not $APIKey) {
       $APIKey = Get-OpenAIAPIKey 
       if ($null -eq $APIKey){
        return
       }
    }

    $header = @{
        "Content-Type" = "application/json";
        "Authorization" = "Bearer $APIKey"
    }
    $body = @{
        "max_tokens" = $MaxTokens;
        "temperature" = $Temperature;
        "n" = $Samples;
    }
    if ($StopSequences) {
        $body["stop"] = $StopSequences
    }

    if (-not [string]::IsNullOrEmpty($EditInstruction)){ $Task = "Edit" }
    $uri = "https://api.openai.com/v1/"
    switch($Task) {
        "Completion" { 
            $uri += "completions";
            $body["model"] = "text-davinci-003";
            $body["prompt"] = $Prompt;
        }
        "Edit" { 
            $uri += "edits";
            $body["model"] = "davinci-instruct-beta";
            $body["input"] = $Prompt;
            $body["instruction"] = $EditInstruction;
        }
        "Chat" { 
            $uri += "chat/completions"
            $body["model"] = "gpt-3.5-turbo"
            if ($ContinueLastConversation -and $Global:LastChatGPTConversation) {
                $body["messages"] = $Global:LastChatGPTConversation + @{
                    role = "user";
                    content = $Prompt
                }
            }
            else{
                $body["messages"] = @(
                    @{role = "system"; content = "You are a helpful assistant"},
                    @{role = "user"; content = $Prompt}
                )
            }
        }
    }
    if ($PSCmdlet.ShouldProcess(`
@"

Uri: $uri
---
Header : $($header | ConvertTo-Json)
---
Body :  $($body | ConvertTo-Json)
---
Expected cost: $($Prompt.Split().Count + $Samples * $MaxTokens)
"@, "Invoke OpenAI API")) {
        $payload = $body | ConvertTo-Json
        try {
            $response = Invoke-WebRequest -Uri:$uri -Method:Post -Headers:$header -Body:$payload
        }
        catch {
            $StatusCode = $_.Exception.Response.StatusCode
            $ErrorMsg = $_.ErrorDetails
            Write-Error "Request failed with code $StatusCode `n$($ErrorMsg | ConvertFrom-Json)"
            return
        }
        $response = $response.Content | ConvertFrom-Json
        Write-Verbose $response | Select-Object -ExpandProperty Usage

        $textResponses = $response.choices | ForEach-Object {
            if ($_ | Get-Member "Message"){
                $_.Message.Content.Trim()
            }
            elseif ($_ | Get-Member "Text"){
                $_.Text.Trim()
            }
        }

        if ($ContinueLastConversation) {
            $Global:LastChatGPTConversation += $body["messages"][-1]
            $Global:LastChatGPTConversation += $response.choices | ForEach-Object {
                @{role=$_.Message.role; content=$_.Message.content}
            }
        }
        else {
            if ($Task -eq "Chat"){
                $Global:LastChatGPTConversation = $body["messages"] + @($response.choices | ForEach-Object {
                    @{role=$_.Message.role; content=$_.Message.content}
                })
            }
        }
        $textResponses | ForEach-Object {
            Write-host "[AI]: $_"
        }
        $Global:AIResponse = $response
        return $null
    }
}

function Set-OpenAIAPIKey {
    param (
        [string] $FilePath = "$PSScriptRoot/OpenAI_API.key"
    )
    $EncryptionKey = [int[]] (Get-Random -SetSeed 2048 -Maximum ([byte]::MaxValue) -Count 16)
    $apiKey = Read-Host -Prompt "Enter your API key: " -AsSecureString 
    $apiKey = ConvertFrom-SecureString -SecureString $apiKey -Key $EncryptionKey
    $apiKey | Set-Content $FilePath
    Write-Host "Success! The API key is encrypted and stored in $FilePath" -ForegroundColor Green
}

function Get-OpenAIAPIKey {
    param (
        [string] $FilePath = "$PSScriptRoot/OpenAI_API.key"
    )
    if (-not (Test-Path $FilePath)) {
        Write-Error "API Key not found: $FilePath `nPlease use Set-OpenAPIKey"
        return $null
    }
    $DecryptionKey = [int[]] (Get-Random -SetSeed 2048 -Maximum ([byte]::MaxValue) -Count 16)
    $key = Get-Content $FilePath | ConvertTo-SecureString -Key $DecryptionKey
    return ConvertFrom-SecureString -SecureString $key -AsPlainText
}
