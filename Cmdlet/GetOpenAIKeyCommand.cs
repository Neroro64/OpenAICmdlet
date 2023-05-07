namespace OpenAICmdlet;

[Cmdlet(VerbsCommon.Get, "OpenAIKey")]
public class GetOpenAIKeyCommand : MyCmdlet
{
    [Parameter(HelpMessage = "The file path where API key is stored")]
    public string Path { get; set; } = SecureAPIKey.DefaultAPIKeyPath;

    protected override void EndProcessing() => WriteObject(SecureAPIKey.DecryptFromFile(Path));
}
