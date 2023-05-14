namespace OpenAICmdlet;

[Cmdlet(VerbsData.Restore, "OpenAIReponseHistory")]
[OutputType(typeof(string))]
public class RestoreOpenAIResponseHistory : MyCmdlet
{
    [Parameter(Mandatory = true)]
    [ValidateSet(nameof(TaskCategory.All), nameof(TaskCategory.Text), nameof(TaskCategory.Image), nameof(TaskCategory.Audio))]
    public TaskCategory CommandCategory { get; init; } = TaskCategory.All;

    // TODO: Add argument completer
    [Parameter(Mandatory = true)]
    public string Path { get; init; } = History.DefaultHistoryPath;

    [Parameter()]
    public SwitchParameter Force { get; init; }

    protected override void EndProcessing()
    {
        if (!File.Exists(Path))
            throw new FileNotFoundException($"Session file not found at {Path}");

        var success = LoadHistoryFromDisk(Path, CommandCategory, Force).Result;
        if (success)
            WriteObject($"Successfully restored the session history from {Path}");
        else
            WriteObject($"Failed to restor the session history from {Path}");
    }

    private static async Task<bool> LoadHistoryFromDisk(string filename, TaskCategory taskCategory, bool force)
    {
        using FileStream fs = File.OpenRead(filename);
        var history = await JsonSerializer.DeserializeAsync<History>(fs).ConfigureAwait(continueOnCapturedContext: false);
        await fs.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);

        if (history == null)
            return false;

        var flag = (int)taskCategory;
        var restoreTextHistory = (flag & 1) != 0;
        var restoreImageHistory = ((flag >>= 1) & 1) != 0;
        var restoreAudioHistory = ((flag >>= 1) & 1) != 0;

        if (restoreTextHistory)
            InvokeOpenAITextCommand.RestoreHistory(history, force);
        if (restoreImageHistory)
            InvokeOpenAIImageCommand.RestoreHistory(history, force);
        if (restoreAudioHistory)
            InvokeOpenAIAudioCommand.RestoreHistory(history, force);

        return true;
    }
}
