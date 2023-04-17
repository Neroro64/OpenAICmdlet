using System.Security.Cryptography;
namespace OpenAICmdlet;

[Cmdlet(VerbsCommon.Get, "OpenAIKey")]
public class GetOpenAIKeyCommand : MyCmdlet
{
    [Parameter()]
    public string Path { get; set; } = SecureAPIKey.DefaultAPIKeyPath;

    protected override void EndProcessing() => WriteObject(SecureAPIKey.DecryptFromFile(Path));
}
