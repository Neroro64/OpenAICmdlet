using System.Security.Cryptography;
namespace OpenAICmdlet;

[Cmdlet(VerbsCommon.Set, "OpenAIAPIKey", SupportsShouldProcess = true,
        ConfirmImpact = ConfirmImpact.High)]
public class SetOpenAIAPIKeyCommand : MyCmdlet
{
    [Parameter()]
    public string Path { get; init; } = GetOpenAIAPIKeyCommand.DefaultAPIKeyPath;

    private readonly byte[] _aesKey = { 149, 71, 43,  220, 145, 121, 185, 194,
                                      133, 27, 170, 147, 4,   138, 64,  178 };
    private readonly byte[] _aesIv = { 182, 224, 103, 188, 126, 187, 117, 69,
                                     143, 231, 15,  212, 123, 18,  9,   106 };

    protected override void EndProcessing()
    {
        bool fileExists = File.Exists(Path);
        string warningText = (fileExists) ? $"Replacing existing API key at {Path}?"
                                          : $"Saving API key to {Path}?";
        if (ShouldProcess(verboseDescription: warningText,
                          verboseWarning: warningText, caption: "Save API Key"))
        {
            if (!fileExists)
                createParentDirectories(Path);

            using (SymmetricAlgorithm aes = Aes.Create())
            {
                aes.Key = _aesKey;
                aes.IV = _aesIv;
                ICryptoTransform encryptor = aes.CreateEncryptor();
                using (FileStream fileStream = File.Open(Path, FileMode.Create))
                {
                    using (CryptoStream cryptoStream = new(
                               fileStream, encryptor,
                               CryptoStreamMode.Write)) using (StreamWriter streamWriter =
                                                                   new(cryptoStream))
                    {
                        streamWriter.Write(ReadConsoleLine(prompt: "Enter your API Key: "));
                        WriteVerbose($"Successfully the API key to {Path}");
                    }
                }
            }
        }
    }

    private void createParentDirectories(string path)
    {
        var parentDir = Directory.GetParent(path);
        if (parentDir != null && !parentDir.Exists)
        {
            createParentDirectories(parentDir.FullName);
            Directory.CreateDirectory(parentDir.FullName);
        }
    }
}
