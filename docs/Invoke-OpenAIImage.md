---
external help file: OpenAICmdlet.dll-Help.xml
Module Name: OpenAICmdlet
online version:
schema: 2.0.0
---

# Invoke-OpenAIImage

## SYNOPSIS
Generates or edits images using OpenAI's image generation model.

## SYNTAX

```
Invoke-OpenAIImage [[-Prompt] <String>] [-Mode <OpenAITask>] [-ImagePath <String>] [-ImageSize <String>]
 [-ImageMaskPath <String>] [-Samples <Int32>] [-APIKeyPath <String>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Invoke-OpenAIImage cmdlet allows you to generate or edit images using OpenAI's image generation model. It provides a flexible interface for generating images based on prompts or performing image edits.

## EXAMPLES

### Example 1: Generate images using a prompt
```powershell
Invoke-OpenAIImage -Prompt "Generate a colorful landscape"
```

This example generates images based on the provided prompt.

### Example 2: Edit an image using a prompt
```powershell
Invoke-OpenAIImage -Prompt "Turn the sky into a sunset"
```

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

### -ImageMaskPath
Path to the image mask file

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

### -ImagePath
Path to the input image file (png)

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

### -ImageSize
The size of the generated image

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: 256x256, 512x512, 1024x1024

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Mode
Image generation mode

```yaml
Type: OpenAITask
Parameter Sets: (All)
Aliases:
Accepted values: ImageGeneration, ImageEdit, ImageVariation

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Prompt
The prompt(s) to generate images or guide image edits

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
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

### System.String
## OUTPUTS

### OpenAI.Response
## NOTES

## RELATED LINKS
