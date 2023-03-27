# OpenAI Cmdlet
A simple PowerShell cmdlet for invoking OpenAI API to perfom and automate text, image and speect-to-text related tasks. 

```powershell
# The following command is equivalent to 
# Invoke-OpenAIText -Prompt "..." -Mode:ChatGPT -Temperature:0 -Samples:1
ai @"
> Convert this text to a programmatic command:
>
> Example: Ask Constance if we need some bread
> Output: send-msg `find constance` Do we need some bread?
>
> Reach out to the ski store and figure out if I can get my skis fixed before I leave on Thursday
> "@
```
```
Prompt   : Convert this text to a programmatic command:

           Example: Ask Constance if we need some bread
           Output: send-msg
                            ind constance Do we need some bread?

           Reach out to the ski store and figure out if I can get my skis fixed before I leave on Thursday
Response : {send-msg ind ski-store Can I get my skis fixed before I leave on Thursday?}
```

For API details, please check [OpenAI API Documentation](https://platform.openai.com/docs/introduction/overview)

For more usage examples, please check [OpenAI API Examples](https://platform.openai.com/examples)

## Requirements
- [OpenAI API Key]( https://openai.com/blog/openai-api )
- [PowerShell >= 7.3.3](https://github.com/PowerShell/powershell/releases)

## Getting started
### Import the module
```powershell
git clone git@github.com:Neroro64/OpenAICmdlet.git
Import-Module OpenAICmdlet/PSCmdlet/InvokeOpenAI.psd1

# To set the API key
Set-OpenAIAPIKey
> Enter your API key: : ********
"Success! The API key is encrypted and stored in $ENV:OPENAI_API_KEY (default: $PSScriptRoot/OpenAI_API.key)"

# You add this module to your PowerShell profile, to be auto-imported upon start-up
# Linux
Add-Content -Path $Home/.config/powershell/Microsoft.PowerShell_profile.ps1 -Value "Import-Module $((Get-Location).Path)/OpenAICmdlet/PSCmdlet/InvokeOpenAI:psd1"
# Windows
Add-Content -Path $Home/Documents/PowerShell/Microsoft.PowerShell_profile.ps1 -Value "Import-Module $((Get-Location).Path)/OpenAICmdlet/PSCmdlet/InvokeOpenAI:psd1"
```

## Usage
NOTE: All responses in the PowerShell session are automatically stored in `$Global:OpenAIResponses` and the last chat conversation is stored in `$Global:LastChatGPTConversation`. So you can always review them and save them to disk if necessary.

Use `Get-Help <CommandName> -Full` to learn more about the syntax of the command and see more examples.

### -WhatIf
You can alwasy preview the content of the API request by appending `-WhatIf` flag to the command (ie. dry-run)

```powershell
ai "Generate prompts for creative ai arts" -WhatIf
```

```
What if: Performing the operation "Invoke OpenAI API" on target "Text completion with maximum estimated tokens: 206 => $2E-06".
What if: Performing the operation "Invoke OpenAI API Request" on target "https://api.openai.com/v1/chat/completions with
---
Header : {
  "Content-Type": "application/json",
  "Authorization": "Bearer <Your API KEY>"
}
---
Body :  {
  "messages": [
    {
      "content": "You are a helpful assistant",
      "role": "system"
    },
    {
      "content": "Generate prompts for creative ai arts",
      "role": "user"
    }
  ],
  "model": "gpt-3.5-turbo",
  "top_p": 1.0,
  "max_tokens": 200,
  "temperature": 0.0,
  "n": 1
}
---
Form :  null
---".
```

### `Invoke-OpenAIText` (Text completion) [Alias='ai']
A simple PowerShell function for invoking OpenAI's API to perform the text related tasks such as
text/code completion, summarize, explanation etc. (default mode: ChatGPT)

For more details, see https://platform.openai.com/docs/guides/completion

By default, it uses ChatGPT mode (model=`gpt-3.5-turbo`) because it perform similar to `text-davinci-003` on general tasks at 10% of the price.

---
#### **Text Generation**
```powershell
ai "Generate prompts for creative ai arts"
```
```
Prompt   : Generate prompts for creative ai arts
Response : {1. Create an abstract painting inspired by the colors of a sunset.
           2. Design a futuristic cityscape using geometric shapes and neon colors.
           3. Generate a portrait of a person using only lines and shapes.
           4. Create a landscape painting of a forest in autumn.
           5. Design a surrealistic scene with floating objects and distorted perspectives.
           6. Generate a digital collage of different textures and patterns.
           7. Create a minimalist illustration of a city skyline at night.
           8. Design a pattern inspired by the shapes and colors of a flower garden.
           9. Generate a digital sculpture of an animal using abstract shapes.
           10. Create a mixed media artwork using both traditional and digital techniques.}
```
---
```powershell
ai -Prompt:"Help me learn dotnet Dependency injection" -Temperature:1 -MaxTokens:500 -Samples:1 -StopSequences:"`n"
```
>Sure, I can help you with that!
>
>In .NET, Dependency Injection (DI) is a design pattern that helps developers achieve loosely coupled code, which is more maintainable, testable, and extensible. DI allows developers to inject dependencies (such as interface contracts) into their code rather than creating the dependencies themselves.
>
>Here is a basic example of how to use DI in C#:
>
>1. Define the interface contract for the dependency:
>
>```c#
>public interface IMyService
>{
>    void DoSomething();
>}
>```
>... 
---
#### To continue chatting

```powershell
ai "Show me more complex examples" -ContinueLastConversation
```
> Sure, here's a more complex example that involves multiple dependencies and configuration:
> 
> 1. Define the interface contracts for the dependencies:
> 
> ```c#
> public interface IMyService1
> {
>     void DoSomething();
> }
> 
> public interface IMyService2
> {
>     void DoSomethingElse();
> }
> ```
> ...


### `Invoke-OpenAIImage` (Image generation) [Alias='aiimg']
A simple PowerShell function for invoking OpenAI's API to perform the image related tasks such as
image generation from text prompt, image edits and image variation. (default mode: Generation)

For more details, see https://platform.openai.com/docs/guides/images/introduction

---

```powershell
aiimg "A happy man eating hot dog" | Select Response -First 1 | Start-process
# The response contains an url to the generated image. By pipling this url to Start-Process we can open the link in a browser
```
![the_generated_image]( resources/a_happy_man_eating_hot_dog.png )

---

```powershell
# Use one of the prompt that we generated above.
aiimg "Create a landscape painting of a forest in autumn." | Select Response -First 1 | Start-process
```
![the_painting]( resources/generated_img.png )

---

```powershell
# Use the generated image as input to generate more variations
aiimg -Mode:Variation -ImagePath:"resources/generated_img.png" -ImageSize:"256x256" | Select Response -First 1 | Start-process
```
![the_painting]( resources/generated_img_variation.png )

### `Invoke-OpenAIAudio` (Speech-to-text) [Alias='aiaudio']
A simple PowerShell function for invoking OpenAI's API to perform the speech-to-text related tasks such as
transcription and translation. (default mode: Transcription)

For details, see https://platform.openai.com/docs/guides/speech-to-text

---

```powershell
aiaudio -Mode:Transcription -AudioPath:"resources/f7879738_nohash_0.wav" -AudioLanguage:en
```

```
Prompt   :
Response : {down,}
```

## Tests
```powershell
# To run the unit tests
Invoke-Pester PSCmdlet/Tests/InvokeOpenAI.Tests.ps1 -Output Detailed
```