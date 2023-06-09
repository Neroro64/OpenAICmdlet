﻿namespace OpenAI;

[Serializable]
public sealed class RequestBody
{
    #region Text parameters
    public string? Prompt { get; set; }
    public string? Model { get; set; }
    public IEnumerable<Dictionary<string, string>>? Messages { get; set; }
    public IEnumerable<string>? Stop { get; set; }
    public string? Suffix { get; set; }
    #endregion

    #region Image parameters
    [JsonIgnore]
    public string? Size { get; set; }
    [JsonIgnore]
    public string? Image { get; set; }
    [JsonIgnore]
    public string? Mask { get; set; }
    #endregion

    #region Audio parameters
    [JsonIgnore]
    public string? File { get; set; }
    public string? Language { get; set; }
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
