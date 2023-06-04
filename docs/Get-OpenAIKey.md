---
external help file: OpenAICmdlet.dll-Help.xml
Module Name: OpenAICmdlet
online version:
schema: 2.0.0
---

# Get-OpenAIKey

## SYNOPSIS
Retrieves the OpenAI API key from the specified file path.

## SYNTAX

```
Get-OpenAIKey [-Path <String>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-OpenAIKey cmdlet retrieves the OpenAI API key from the specified file path. This API key is used for authentication and authorization when interacting with OpenAI's services.

## EXAMPLES

### Example 1: Retrieve the API key from the default path
```powershell
Get-OpenAIKey
```

This example retrieves the OpenAI API key from the default file path and returns it.

### Example 2: Retrieve the API key from a custom file path
```powershell
Get-OpenAIKey -Path "C:\Keys\openai_key.txt"
```

This example retrieves the OpenAI API key from the specified file path `"C:\Keys\openai_key.txt"` and returns it.

## PARAMETERS

### -Path
Specifies the file path where the API key is stored. If not provided, the default API key path, SecureAPIKey.DefaultAPIKeyPath, is used. The default value ensures seamless retrieval of the API key.

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
## OUTPUTS

### System.String

## NOTES
The API key is necessary for authentication and authorization when interacting with OpenAI's services. Keep it secure and avoid sharing it unintentionally.

## RELATED LINKS

[Set-OpenAIKey](Set-OpenAIKey.md)
