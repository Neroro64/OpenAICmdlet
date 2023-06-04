---
external help file: OpenAICmdlet.dll-Help.xml
Module Name: OpenAICmdlet
online version:
schema: 2.0.0
---

# Set-OpenAIKey

## SYNOPSIS
Sets the OpenAI API key for authentication.

## SYNTAX

```
Set-OpenAIKey [-Path <String>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `Set-OpenAIKey` cmdlet allows you to set the OpenAI API key used for authentication. This key is required to access OpenAI services and must be securely stored.

## EXAMPLES

### Example 1 - Set the OpenAI API key
```powershell
Set-OpenAIKey -Path "C:\Keys\openai.key"
```

This example sets the OpenAI API key and stores it in the specified file path.

### Example 2 - Set the OpenAI API key using the default path
```powershell
Set-OpenAIKey
```

This example sets the OpenAI API key and stores it in the default file path.

## PARAMETERS

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

### -Path
The file path where to store the API key

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

### None
### The OpenAI API key is required for authentication to access OpenAI services.
### Ensure that you store the API key in a secure location to prevent unauthorized access.
### To obtain an API key, sign up for an account on the OpenAI website and follow the instructions to generate a key.

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
