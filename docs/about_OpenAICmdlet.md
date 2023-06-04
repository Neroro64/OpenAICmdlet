# OpenAICmdlet
## about_OpenAICmdlet

```
ABOUT TOPIC NOTE:
The first header of the about topic should be the topic name.
The second header contains the lookup name used by the help system.

IE:
# Some Help Topic Name
## SomeHelpTopicFileName

This will be transformed into the text file
as `about_SomeHelpTopicFileName`.
Do not include file extensions.
The second header should have no spaces.
```

# SHORT DESCRIPTION
This PowerShell module provides essential cmdlets for seamless interaction with OpenAI's models.

```
ABOUT TOPIC NOTE:
About topics can be no longer than 80 characters wide when rendered to text.
Any topics greater than 80 characters will be automatically wrapped.
The generated about topic will be encoded UTF-8.
```

# LONG DESCRIPTION
The `OpenAICmdlet` PowerShell module offers a comprehensive set of cmdlets designed to facilitate effortless integration with OpenAI's powerful models. With this module, you gain access to essential functionalities through a range of versatile cmdlets.

The 'Get-OpenAIKey' cmdlet allows you to retrieve the OpenAI API key currently set in your environment, providing seamless access to OpenAI's services.

By utilizing the 'Set-OpenAIKey' cmdlet, you can easily configure and update your OpenAI API key, ensuring uninterrupted connectivity to OpenAI's models.

Harness the power of language generation with the 'Invoke-OpenAIText' cmdlet, which enables you to generate text using OpenAI's models. Effortlessly create and customize content for various applications such as chatbots, language translation, and text completion.

With the 'Invoke-OpenAIImage' cmdlet, effortlessly generate and manipulate images using OpenAI's models. From artistic style transfer to content generation, this cmdlet unlocks a world of creative possibilities.

For audio-related tasks, the 'Invoke-OpenAIAudio' cmdlet provides seamless integration with OpenAI's models, allowing you to generate, modify, and enhance audio content. Whether it's text-to-speech conversion or translation, this cmdlet offers versatility and flexibility.


# EXAMPLES
## Text generation
```powershell
(igpt) -> Invoke-OpenAIText "Generate prompts for creative ai arts"
```
## Image generation
```powershell
(idalle) -> Invoke-OpenAIImage "A happy man eating hot dog" | Select Response -First 1 | Start-process
# The response contains an url to the generated image. By pipling this url to Start-Process we can open the link in a browser
```
## Audio transcription
```powershell
(iwhisper) -> Invoke-OpenAIAudio -AudioPath:"resources/f7879738_nohash_0.wav" -AudioLanguage:en
```