namespace OpenAICmdlet;

[Serializable]
public sealed class OpenAIRequestBody
{
    #region Text parameters
    public string? Prompt;
    public string? Model;
    public List<Dictionary<string, string>>? Messages;
    public string[]? Stop;
    public string? Suffix;
    #endregion

    #region Image parameters
    public string? Size;
    public string? Image;
    public string? Mask;
    #endregion

    #region Audio parameters 
    public string? File;
    #endregion

    #region Generation parameters 
    public float Temperature = 1;
    public float Top_p = 1;
    public int Max_Tokens = 200;
    public int N = 1;
    public float Precence_penalty = 0;
    public float Frequency_penalty = 0;
    #endregion

}