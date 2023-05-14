namespace OpenAI;
[Serializable]
public class History : List<Session>
{
    public static string DefaultHistoryPath { get; } = System.IO.Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OpenAICmdlet/SessionHistory");
}

[Serializable]
public class Session : List<Response> { }
