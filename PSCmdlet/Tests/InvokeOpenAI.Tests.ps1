BeforeAll {
    Import-Module "$PSScriptRoot/../InvokeOpenAI.psm1" -force
    $ErrorActionPreference = 'Break'
}

Describe "Store and Load API Key" {
    BeforeEach {
        Mock Read-Host { return "ABCD1234" | ConvertTo-SecureString }
    }

    It "Store the key" {
        Mock Set-Content -Verifiable -MockWith { $global:content = $_; return $null }
        Set-OpenAIAPIKey
        Should -InvokeVerifiable
    }
    It "Load the key" {
        Mock Get-Content -Verifiable -MockWith { return $global:content }
        Get-OpenAIAPIKey | Should -Not -BeNullOrEmpty
        Should -InvokeVerifiable
    }
}

Describe "Invoke tasks" {
    BeforeEach {
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

    It "Can invoke commands" -ForEach @(
        @{ Params = @{Task = "Text"; Prompt = "Say this is a test"; Temperature = 0; MaxTokens = 7; Samples = 1; StopSequences = "\n" } },
        @{ Params = @{Task = "Chat"; Prompt = "Hello!"; Temperature = 0; MaxTokens = 7; Samples = 1; StopSequences = "\n" } },
        @{ Params = @{Task = "Image"; Prompt = "A cute baby sea otter"; ImageSize = "512x512"; Samples = 1 } },
        @{ Params = @{Task = "ImageEdit"; Prompt = "A cute baby sea otter wearing a beret"; ImagePath = "otter.png"; ImageMaskPath = "mask.png"; Samples = 1; ImageSize = "512x512" } },
        @{ Params = @{Task = "ImageVariation"; ImagePath = "otter.png"; ImageSize = "512x512"; Samples = 1 } },
        @{ Params = @{Task = "Transcription"; AudioPath = "audio.mp3"; AudioLanguage = "English"; Temperature = 1 } },
        @{ Params = @{Task = "Translation"; AudioPath = "audio.mp3"; Temperature = 1 } }
    ) {
        $response = Invoke-OpenAI @Params
        Should -InvokeVerifiable
        $response | Should -not -BeNullOrEmpty
        $response.Response | Should -not -BeNullOrEmpty
    }

    It "Can continue last chat" {
        $response = Invoke-OpenAI -Prompt:"Hello!" -Task:Chat -Temperature:0 -MaxTokens:200 -Samples 1
        $msgCount = $Global:LastChatGPTConversation.Count
        $response2 = Invoke-OpenAI -Prompt:"Hello Again!" -Task:Chat -Temperature:0 -MaxTokens:200 -Samples 1 -ContinueLastConversation
        Should -InvokeVerifiable
        $response | Should -not -BeNullOrEmpty
        $response2 | Should -not -BeNullOrEmpty
        $Global:LastChatGPTConversation.Count  | Should -BeGreaterThan $msgCount

    }
}