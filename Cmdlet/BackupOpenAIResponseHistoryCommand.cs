namespace OpenAICmdlet;

[Cmdlet(VerbsData.Backup, "OpenAIReponseHistory")]
[OutputType(typeof(string))]
public class BackupOpenAIResponseHistory : MyCmdlet
{
    [Parameter()]
    [ValidateSet(nameof(TaskCategory.All), nameof(TaskCategory.Text), nameof(TaskCategory.Image), nameof(TaskCategory.Audio))]
    public TaskCategory CommandCategory { get; init; } = TaskCategory.All;
    [Parameter()]
    public string Path { get; init; } = History.DefaultHistoryPath;

    [Parameter()]
    public SwitchParameter ClearCache { get; init; }

    protected override void EndProcessing()
    {
        var filename = "";
        var flag = (int)CommandCategory;
        var backupTextHistory = (flag & 1) != 0;
        var backupImageHistory = ((flag >>= 1) & 1) != 0;
        var backupAudioHistory = ((flag >>= 1) & 1) != 0;
        if (backupTextHistory)
        {
            filename = System.IO.Path.Join(
                Path,
                $"GPT_{DateTime.Now.ToString("yyyy-MMdd-HH-mm", System.Globalization.CultureInfo.InvariantCulture)}.json");
            WriteHistoryToDisk(InvokeOpenAITextCommand.History, filename);
            if (ClearCache)
                InvokeOpenAITextCommand.RestoreHistory(new History(), force: true);
        }
        if (backupImageHistory)
        {

            filename = System.IO.Path.Join(
                Path,
                $"DALLE_{DateTime.Now.ToString("yyyy-MMdd-HH-mm", System.Globalization.CultureInfo.InvariantCulture)}.json");
            WriteHistoryToDisk(InvokeOpenAIImageCommand.History, filename);
            if (ClearCache)
                InvokeOpenAIImageCommand.RestoreHistory(new History(), force: true);
        }
        if (backupAudioHistory)
        {
            filename = System.IO.Path.Join(
                Path,
                $"Whisper_{DateTime.Now.ToString("yyyy-MMdd-HH-mm", System.Globalization.CultureInfo.InvariantCulture)}.json");
            WriteHistoryToDisk(InvokeOpenAIAudioCommand.History, filename);
            if (ClearCache)
                InvokeOpenAIAudioCommand.RestoreHistory(new History(), force: true);
        }
        WriteObject($"Successfully saved the session history to {Path}");
    }

    private async static void WriteHistoryToDisk(IEnumerable<IEnumerable<Response>> history, string filename)
    {
        if (!System.IO.Directory.Exists(filename))
            filename.CreateParentDirectories();
        using FileStream fs = File.Create(filename);
        await JsonSerializer.SerializeAsync(fs, history).ConfigureAwait(continueOnCapturedContext: false);
        await fs.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
    }
}
