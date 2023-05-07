using System.Security.Cryptography;
namespace OpenAICmdlet;

internal static class SecureAPIKey
{
    private static ReaderWriterLock _rwLock = new();
    internal static string DefaultAPIKeyPath =
        System.Environment.GetEnvironmentVariable("OPENAI_APIKEY") ??
        System.IO.Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OpenAICmdlet/API.key");

    internal static void EncryptToFile(string path, string apiKey)
    {
        if (!File.Exists(path))
            path.CreateParentDirectories();

        using (Aes aes = Aes.Create())
        {
            (aes.Key, aes.IV) = aesInitPair;
            ICryptoTransform encryptor = aes.CreateEncryptor();
            _rwLock.AcquireWriterLock(Constant.RW_LOCK_TIMEOUT_MS);
            try
            {
                using (FileStream fileStream = File.Open(path, FileMode.Create))
                {
                    using (var cryptoStream = new CryptoStream(
                               fileStream, encryptor,
                               CryptoStreamMode.Write)) using (var streamWriter =
                                                                   new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(apiKey);
                    }
                }
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }
    }

    internal static string DecryptFromFile(string? path)
    {
        path ??= DefaultAPIKeyPath;
        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"API key not found at {path}! \nUse Set-OpenAIkey cmdlet to save your API key.");

        using (Aes aes = Aes.Create())
        {
            (aes.Key, aes.IV) = aesInitPair;
            ICryptoTransform decryptor = aes.CreateDecryptor();
            _rwLock.AcquireWriterLock(Constant.RW_LOCK_TIMEOUT_MS);
            try
            {
                using (FileStream fileStream = File.OpenRead(path))
                {
                    using (var cryptoStream = new CryptoStream(
                               fileStream, decryptor,
                               CryptoStreamMode.Read)) using (var streamReader =
                                                                  new StreamReader(cryptoStream))
                    {
                        string apiKey = streamReader.ReadToEnd();
                        return apiKey;
                    }
                }
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }
    }
    private static readonly Lazy<Random> _randomGenerator =
        new(() => new Random(System.Reflection.Assembly.GetExecutingAssembly()
                                 .GetName()
                                 .Version?.ToString()
                                 .GetHashCode(StringComparison.InvariantCultureIgnoreCase) ??
                             Constant.MAGIC_SEED));

    private static (byte[], byte[]) aesInitPair => (_aesKey.Value, _aesIV.Value);
    private static readonly Lazy<byte[]> _aesKey = new(() => generateRandomByteArray());
    private static readonly Lazy<byte[]> _aesIV = new(() => generateRandomByteArray());

    private static byte[] generateRandomByteArray()
    {
        var buffer = new byte[16];
        _randomGenerator.Value.NextBytes(buffer);
        return buffer;
    }
}
