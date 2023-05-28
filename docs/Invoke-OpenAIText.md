---
external help file: OpenAICmdlet.dll-Help.xml
Module Name: OpenAICmdlet
online version:
schema: 2.0.0
---

# Invoke-OpenAIText

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

```
Invoke-OpenAIText [-Prompt] <String> [-Mode <OpenAITask>] [-ContextFilePath <String>] [-Suffix <String>]
 [-MaxTokens <Int32>] [-Temperature <Single>] [-Top_p <Single>] [-PrecencePenalty <Single>]
 [-FrequencyPenalty <Single>] [-ChatInitInstruction <String>]
 [-StopSequences <System.Collections.Generic.IEnumerable`1[System.String]>]
 [-ContinueSession <System.Collections.Generic.ICollection`1[OpenAI.Response]>] [-Samples <Int32>]
 [-APIKeyPath <String>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
{{ Fill in the Description }}

## EXAMPLES

### Example 1
```powershell
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

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

### -ChatInitInstruction
This instruction sets the initial setting of the chat model

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

### -ContextFilePath
Path to a text file with extra context

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ContinueSession
A session with previous response exchanges

```yaml
Type: System.Collections.Generic.ICollection`1[OpenAI.Response]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FrequencyPenalty
Positive values penalize new tokens based on their existing frequency in the text so far, decreasing the model's likelihood to repeat the same line verbatim.

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

### -MaxTokens
The maximum number of tokens to generate in the completion.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Mode
Text completion mode. 
Note:'ChatGPT' performs similar to 'TextCompletion' at 10% the price.

```yaml
Type: OpenAITask
Parameter Sets: (All)
Aliases:
Accepted values: ChatCompletion, TextCompletion

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrecencePenalty
Positive values penalize new tokens based on whether they appear in the text so far, increasing the model's likelihood to talk about new topics.

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

### -Prompt
The prompt(s) to generate completions for, encoded as a string

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

### -Samples
Number of images that should be generated

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StopSequences
Up to 4 sequences where the API will stop generating further tokens.
The returned text will not contain the stop sequence.

```yaml
Type: System.Collections.Generic.IEnumerable`1[System.String]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Suffix
The suffix that comes after a completion of inserted text.

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

### -Top_p
An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass.
So 0.1 means only the tokens comprising the top 10% probability mass are considered.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
## OUTPUTS

### OpenAI.Response
## NOTES

## RELATED LINKS
