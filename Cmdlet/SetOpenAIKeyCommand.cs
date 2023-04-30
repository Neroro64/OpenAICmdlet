namespace OpenAICmdlet;

[Cmdlet(VerbsCommon.Set, "OpenAIKey", SupportsShouldProcess = true,
        ConfirmImpact = ConfirmImpact.High)]
public class SetOpenAIKeyCommand : MyCmdlet
{
    [Parameter()]
    public string Path { get; init; } = SecureAPIKey.DefaultAPIKeyPath;

    protected override void EndProcessing()
    {
        bool fileExists = File.Exists(Path);
        string warningText = (fileExists) ? $"Replacing existing API key at {Path}?"
                                          : $"Saving API key to {Path}?";
        if (ShouldProcess(verboseDescription: warningText,
                          verboseWarning: warningText, caption: "Save API Key"))
        {
            SecureAPIKey.EncryptToFile(Path, ReadConsoleLine(prompt: "Enter your API Key: "));
        }
    }

}
