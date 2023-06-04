---
external help file: OpenAICmdlet.dll-Help.xml
Module Name: OpenAICmdlet
online version:
schema: 2.0.0
---

# Invoke-OpenAIAudio

## SYNOPSIS
Executes audio-related tasks using OpenAI's models.

## SYNTAX

```
Invoke-OpenAIAudio [-AudioPath] <String> [-Mode <OpenAITask>] [-Prompt <String>] [-AudioLanguage <String>]
 [-Temperature <Single>] [-APIKeyPath <String>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION
The `Invoke-OpenAIAudio` cmdlet allows you to perform audio-related tasks using OpenAI's models. It leverages the power of deep learning to provide speech-to-text transcription or audio translation capabilities.

## EXAMPLES

### Example 1: Perform speech-to-text transcription
```powershell
Invoke-OpenAIAudio -AudioPath "C:\Audio\speech.mp3" -Mode AudioTranscription
```

This example transcribes the speech from the audio file located at "C:\Audio\speech.mp3" using OpenAI's models.

### Example 2: Perform audio translation
```powershell
Invoke-OpenAIAudio -AudioPath "C:\Audio\conversation.wav" -Mode AudioTranslation -AudioLanguage "en" -Prompt "Translate the conversation from English to French."
```

This example translates the conversation in the audio file located at "C:\Audio\conversation.wav" from English to French using OpenAI's models. The `-AudioLanguage` parameter specifies the input audio language as English, and the `-Prompt` parameter provides a prompt to guide the translation process.

### Example 3: Specify custom API key path
```powershell
Invoke-OpenAIAudio -AudioPath "C:\Audio\audio.mp4" -APIKeyPath "C:\Keys\openai_key.txt"
```

This example processes the audio file located at "C:\Audio\audio.mp4" using OpenAI's models. It specifies a custom API key path by using the `-APIKeyPath` parameter, which points to the file containing the OpenAI API key.

Note: Ensure that you replace the file paths and language settings with appropriate values based on your requirements.

## PARAMETERS

### -APIKeyPath
Path to OpenAI API key file

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -AudioLanguage
The language of the input audio.
Supplying the input language in ISO-639-1 format will improve accuracy and latency.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -AudioPath
Path to the input audio file

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Mode
Speect-to-text task

```yaml
Type: OpenAITask
Parameter Sets: (All)
Aliases:
Accepted values: AudioTranscription, AudioTranslation

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Prompt
The prompt(s) to guide the mode's style or continue on previous audio segment

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Temperature
What sampling temperature to use, between 0 and 2.
Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.

```yaml
Type: Single
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES
This cmdlet requires a valid OpenAI API key for authentication and authorization. If the API key is not provided via the `APIKeyPath` parameter, the cmdlet uses the default API key.

## RELATED LINKS

[Set-OpenAIKey](Set-OpenAIKey.md)

