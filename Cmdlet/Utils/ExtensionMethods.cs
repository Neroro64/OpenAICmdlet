namespace OpenAICmdlet;
public static class ExtensionMethods
{
    public static void ThrowIfFileNotFound(this string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(path);
        }
    }
}
