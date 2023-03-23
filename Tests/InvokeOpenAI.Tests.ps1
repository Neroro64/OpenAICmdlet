BeforeAll{
    . "$PSScriptRoot/../InvokeOpenAI.ps1" -force
}

Describe "Store and Load API Key" {
    BeforeEach {
        Mock Read-Host {return "ABCD1234" | ConvertTo-SecureString}
    }

    It "Store the key" {
        Mock Set-Content -Verifiable -MockWith {$global:content = $_; return $null}
        Set-OpenAIAPIKey
        Should -InvokeVerifiable
    }
    It "Load the key" {
        Mock Get-Content -Verifiable -MockWith {return $global:content}
        Get-OpenAIAPIKey | Should -Not -BeNullOrEmpty
        Should -InvokeVerifiable
    }
}

Describe "Text related commands" {
    BeforeEach {
        Mock Invoke-WebRequest -Verifiable -MockWith {
            switch ($body["model"]){
                "text-davinci-003" {return Get-Content "$PSScriptRoot/MockCompletionResponse.json" | ConvertFrom-Json}
                "text-davinci-edit-001" {return Get-Content "$PSScriptRoot/MockEditResponse.json" | ConvertFrom-Json}
                "gpt-3.5-turbo" {return Get-Content "$PSScriptRoot/MockChatResponse.json" | ConvertFrom-Json}
            }
        }
        Mock Get-OpenAIAPIKey -Verifiable -MockWith {return "ABCD1234"}
    }

    It "Simple text completion" -ForEach @(
        @{Task = "Completion"; Prompt = "Say this is a test"},
        @{Task = "Chat"; Prompt = "Hello!" },
        @{Task = "Edit"; Prompt = "What day of the wek is it?"; Instruction = "Fix the spelling mistakes" }
    ){
        if ($Instruction){
            $response = Invoke-OpenAIText -Prompt:$Prompt -EditInstruction $Instruction -Temperature:0 -MaxTokens:200 -Samples 1
        }
        else {
            $response = Invoke-OpenAIText -Prompt:$Prompt -Task:$Task -Temperature:0 -MaxTokens:7 -Samples 1 -StopSequences "\n"
        }
        Should -InvokeVerifiable
        $response | Should -not -BeNullOrEmpty
    }

    It "Chat continued" {
        $response = Invoke-OpenAIText -Prompt:"Hello!" -Task:Chat -Temperature:0 -MaxTokens:200 -Samples 1
        $msgCount = $Global:LastChatGPTConversation.Count
        $response2 = Invoke-OpenAIText -Prompt:"Hello Again!" -Task:Chat -Temperature:0 -MaxTokens:200 -Samples 1 -ContinueLastConversation
        Should -InvokeVerifiable
        $response | Should -not -BeNullOrEmpty
        $response2 | Should -not -BeNullOrEmpty
        $Global:LastChatGPTConversation.Count  | Should -BeGreaterThan $msgCount

    }
}