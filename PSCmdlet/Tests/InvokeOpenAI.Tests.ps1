BeforeAll {
    Import-Module "$PSScriptRoot/../InvokeOpenAI.psm1" -force
    $ErrorActionPreference = 'Break'
}

Describe "OpenAPI Key" -Tag "APIKEY" {
    It "Can be stored" {
        Mock Read-Host -MockWith { return "ABCD1234" | ConvertTo-SecureString }
        Mock Set-Content -Verifiable -MockWith { $global:content = $_; return $null }
        Mock Read-Host {}
        Set-OpenAIAPIKey
        Should -InvokeVerifiable
    }
    It "Can be loaded" {
        Mock Get-Content -Verifiable -MockWith { return $global:content }
        Get-OpenAIAPIKey | Should -Not -BeNullOrEmpty
        Should -InvokeVerifiable
    }
}

Describe "OpenAI API Cmdlets" -Tag "TASKS" {
    BeforeAll {
        Mock Invoke-WebRequest -Verifiable -MockWith {
            switch -Wildcard ($Uri) {
                "*chat/completions" { return Get-Content "$PSScriptRoot/MockCompletionResponse.json" | ConvertFrom-Json }
                "*completions" { return Get-Content "$PSScriptRoot/MockChatResponse.json" | ConvertFrom-Json }
                "*images*" { return Get-Content "$PSScriptRoot/MockImageResponse.json" | ConvertFrom-Json }
                "*audio*" { return Get-Content "$PSScriptRoot/MockAudioResponse.json" | ConvertFrom-Json }
            }
        }
        Mock Get-OpenAIAPIKey -Verifiable -MockWith { return "ABCD1234" | ConvertTo-SecureString }
        Mock Get-Item -MockWith { return "FileContent" }
        Mock Test-Path -MockWith { return $true }
    }

    BeforeEach {
        $Global:OpenAIResponses = $null
    }

    It "Is -WhatIf message correct" -ForEach @(
        @{ Params = @{Mode = "TextCompletion"; Prompt = "Say this is a test"; Temperature = 0; MaxTokens = 7; Samples = 1; StopSequences = "\n" } }
    ) {
        Invoke-OpenAIText @Params -WhatIf
    }

    It "Can perform text completion" -ForEach @(
        @{ Params = @{Mode = "TextCompletion"; Prompt = "Say this is a test"; Temperature = 0; MaxTokens = 7; Samples = 1; StopSequences = "\n" } },
        @{ Params = @{Mode = "ChatGPT"; Prompt = "Hello!"; Temperature = 0; MaxTokens = 7; Samples = 1; StopSequences = "\n" } }
    ) {
        $response = Invoke-OpenAIText @Params
        Should -InvokeVerifiable
        $response | Should -not -BeNullOrEmpty
        $response.Response | Should -not -BeNullOrEmpty
        $Global:OpenAIResponses | Should -not -BeNullOrEmpty
    }

    It "Can perform image generation" -ForEach @(
        @{ Params = @{Mode = "Generation"; Prompt = "A cute baby sea otter"; ImageSize = "512x512"; Samples = 1 } },
        @{ Params = @{Mode = "Edit"; Prompt = "A cute baby sea otter wearing a beret"; ImagePath = "otter.png"; ImageMaskPath = "mask.png"; Samples = 1; ImageSize = "512x512" } },
        @{ Params = @{Mode = "Variation"; ImagePath = "otter.png"; ImageSize = "512x512"; Samples = 1 } }
    ) {
        $response = Invoke-OpenAIImage @Params
        Should -InvokeVerifiable
        $response | Should -not -BeNullOrEmpty
        $response.Response | Should -not -BeNullOrEmpty
        $Global:OpenAIResponses | Should -not -BeNullOrEmpty
    }
    
    It "Can perform audio tasks" -ForEach @(
        @{ Params = @{Mode = "Transcription"; AudioPath = "audio.mp3"; AudioLanguage = "English"; Temperature = 1 } },
        @{ Params = @{Mode = "Translation"; AudioPath = "audio.mp3"; Temperature = 1 } }
    ) {
        $response = Invoke-OpenAIAudio @Params
        Should -InvokeVerifiable
        $response | Should -not -BeNullOrEmpty
        $response.Response | Should -not -BeNullOrEmpty
        $Global:OpenAIResponses | Should -not -BeNullOrEmpty
    }

    It "Can continue on the most recent chat session" {
        $response = Invoke-OpenAIText -Prompt:"Hello!" -Task:Chat -Temperature:0 -MaxTokens:200 -Samples 1
        $msgCount = $Global:LastChatGPTConversation.Count
        $response2 = Invoke-OpenAIText -Prompt:"Hello Again!" -Task:Chat -Temperature:0 -MaxTokens:200 -Samples 1 -ContinueLastConversation
        Should -InvokeVerifiable
        $response | Should -not -BeNullOrEmpty
        $response2 | Should -not -BeNullOrEmpty
        $Global:LastChatGPTConversation.Count  | Should -BeGreaterThan $msgCount
    }
}