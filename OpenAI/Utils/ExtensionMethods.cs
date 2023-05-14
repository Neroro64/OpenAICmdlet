namespace OpenAI;
public static class ExtensionMethods
{
    public static void ThrowIfFileNotFound(this string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(path);
        }
    }
    public static void CreateParentDirectories(this string path)
    {
        var parentDir = Directory.GetParent(path);
        if (parentDir != null && !parentDir.Exists)
        {
            parentDir.FullName.CreateParentDirectories();
            Directory.CreateDirectory(parentDir.FullName);
        }
    }
}
