namespace OpenAICmdlet;

[Serializable]
public sealed class OpenAIRequestBody
{
    #region Text parameters
    public string? Prompt { get; set; }
    public string? Model { get; set; }
    public IEnumerable<Dictionary<string, string>>? Messages { get; set; }
    public IEnumerable<string>? Stop { get; set; }
    public string? Suffix { get; set; }
    #endregion

    #region Image parameters
    public string? Size { get; set; }
    public string? Image { get; set; }
    public string? Mask { get; set; }
    #endregion

    #region Audio parameters
    public string? File { get; set; }
    #endregion

    #region Generation parameters
    public float Temperature { get; set; } = 1;
    public float Top_p { get; set; } = 1;
    public int Max_Tokens { get; set; } = 200;
    public int N { get; set; } = 1;
    public float Precence_penalty { get; set; }
    public float Frequency_penalty { get; set; }
    #endregion
}
