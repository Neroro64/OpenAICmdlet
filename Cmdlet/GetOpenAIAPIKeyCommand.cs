using System.Security.Cryptography;
namespace OpenAICmdlet;

[Cmdlet(VerbsCommon.Get, "OpenAIAPIKey")]
public class GetOpenAIAPIKeyCommand : MyCmdlet
{
    [Parameter()]
    public string Path { get; init; } =
      System.Environment.GetEnvironmentVariable("OPENAI_APIKEY_LOCATION") ??
      System.IO.Path.Join(
          Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
          "OpenAICmdlet/API.key");

    private readonly byte[] _aesKey = { 149, 71, 43, 220, 145, 121, 185, 194, 133, 27, 170, 147, 4, 138, 64, 178 };
    private readonly byte[] _aesIv = { 182, 224, 103, 188, 126, 187, 117, 69, 143, 231, 15, 212, 123, 18, 9, 106 };

    protected override void EndProcessing()
    {
        if (!File.Exists(Path))
            throw new FileNotFoundException($"API Key not found at {Path}");

        using (SymmetricAlgorithm aes = Aes.Create())
        {
            aes.Key = _aesKey;
            aes.IV = _aesIv;
            ICryptoTransform decryptor = aes.CreateDecryptor();
            using (FileStream fileStream = File.OpenRead(Path))
            {
                using (CryptoStream cryptoStream = new(fileStream, decryptor, CryptoStreamMode.Read))
                using (StreamReader streamReader = new(cryptoStream))
                {
                    string apiKey = streamReader.ReadToEnd();
                    WriteObject(apiKey);
                }
            }
        }
    }
}
