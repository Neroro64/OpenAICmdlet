using System.Security.Cryptography;
using OpenAICmdlet;

internal static class SecureAPIKey
{
    internal static string DefaultAPIKeyPath =
    System.Environment.GetEnvironmentVariable("OPENAI_APIKEY")
    ?? System.IO.Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OpenAICmdlet/API.key"
    );

    internal static void EncryptToFile(string path, string apiKey)
    {
        if (!File.Exists(path))
            createParentDirectories(path);

        using (SymmetricAlgorithm aes = Aes.Create())
        {
            (aes.Key, aes.IV) = aesInitPair;
            ICryptoTransform encryptor = aes.CreateEncryptor();
            using (FileStream fileStream = File.Open(path, FileMode.Create))
            {
                using (var cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write))
                using (var streamWriter = new StreamWriter(cryptoStream))
                {
                    streamWriter.Write(apiKey);
                }
            }
        }
    }

    internal static string DecryptFromFile(string? path)
    {
        path ??= DefaultAPIKeyPath;
        if (!File.Exists(path))
            throw new FileNotFoundException($"API key not found at {path}! \nUse Set-OpenAIkey cmdlet to save your API key.");

        using (SymmetricAlgorithm aes = Aes.Create())
        {
            (aes.Key, aes.IV) = aesInitPair;
            ICryptoTransform decryptor = aes.CreateDecryptor();
            using (FileStream fileStream = File.OpenRead(path))
            {
                using (var cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read))
                using (var streamReader = new StreamReader(cryptoStream))
                {
                    string apiKey = streamReader.ReadToEnd();
                    return apiKey;
                }
            }
        }

    }
    private static readonly Lazy<Random> _randomGenerator = new(() =>
        new Random(
            System.Reflection.Assembly
            .GetExecutingAssembly()
            .GetName().Version?.ToString()
            .GetHashCode() ?? 3374));

    private static (byte[], byte[]) aesInitPair => (_aesKey.Value, _aesIV.Value);
    private static readonly Lazy<byte[]> _aesKey = new(() => generateRandomByteArray());
    private static readonly Lazy<byte[]> _aesIV = new(() => generateRandomByteArray());

    private static byte[] generateRandomByteArray()
    {
        var buffer = new byte[16];
        _randomGenerator.Value.NextBytes(buffer);
        return buffer;
    }

    private static void createParentDirectories(string path)
    {
        var parentDir = Directory.GetParent(path);
        if (parentDir != null && !parentDir.Exists)
        {
            createParentDirectories(parentDir.FullName);
            Directory.CreateDirectory(parentDir.FullName);
        }
    }
}