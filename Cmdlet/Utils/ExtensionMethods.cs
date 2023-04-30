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
 
    public static void AddFileContent(this MultipartFormDataContent formContent, string name, string filePath) 
    {
        if (formContent == null)
            return;
        using (FileStream fs = File.OpenRead(filePath))
        using (StreamContent content = new StreamContent(fs))
        formContent.Add(content, name, filePath);
    }
}
