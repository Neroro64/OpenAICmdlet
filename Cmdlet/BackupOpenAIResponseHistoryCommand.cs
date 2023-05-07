namespace OpenAICmdlet;

[Cmdlet(VerbsData.Backup, "OpenAIReponseHistory")]
[OutputType(typeof(string))]
public class BackupOpenAIResponseHistory : MyCmdlet
{
    private static string DefaultHistoryPath = System.IO.Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OpenAICmdlet/SessionHistory");

    [Parameter()]
    [ValidateSet(nameof(OpenAICategory.All), nameof(OpenAICategory.Text), nameof(OpenAICategory.Image), nameof(OpenAICategory.Audio))]
    public OpenAICategory CommandCategory { get; init; } = OpenAICategory.All;
    [Parameter()]
    public string Path { get; init; } = DefaultHistoryPath;

    protected override void EndProcessing()
    {
        var filename = "";
        bool backupTextHistory = false, backupImgHistory = false, backupAudioHistory = false;
        switch (CommandCategory)
        {
            case OpenAICategory.All:
                backupTextHistory = backupImgHistory = backupAudioHistory = true;
                break;
            case OpenAICategory.Text:
                backupTextHistory = true;
                break;
            case OpenAICategory.Image:
                backupImgHistory = true;
                break;
            case OpenAICategory.Audio:
                backupAudioHistory = true;
                break;
        }

        if (backupTextHistory)
        {
            filename = System.IO.Path.Join(
                Path,
                $"GPT_{DateTime.Now.ToString("yyyy-MMdd-HH-mm", System.Globalization.CultureInfo.InvariantCulture)}.json");
            WriteHistoryToDisk(InvokeOpenAITextCommand.History, filename);
        }
        if (backupImgHistory)
        {

            filename = System.IO.Path.Join(
                Path,
                $"DALLE_{DateTime.Now.ToString("yyyy-MMdd-HH-mm", System.Globalization.CultureInfo.InvariantCulture)}.json");
            WriteHistoryToDisk(InvokeOpenAIImageCommand.History, filename);
        }
        if (backupAudioHistory)
        {
            filename = System.IO.Path.Join(
                Path,
                $"Whisper_{DateTime.Now.ToString("yyyy-MMdd-HH-mm", System.Globalization.CultureInfo.InvariantCulture)}.json");
            WriteHistoryToDisk(InvokeOpenAIAudioCommand.History, filename);
        }
        WriteObject($"Successfully saved the session history to {Path}");
    }

    private async static void WriteHistoryToDisk(IEnumerable<IEnumerable<OpenAIResponse>> history, string filename)
    {
        if (!System.IO.Directory.Exists(filename))
            filename.CreateParentDirectories();
        using FileStream fs = File.Create(filename);
        await JsonSerializer.SerializeAsync(fs, history).ConfigureAwait(continueOnCapturedContext: false);
        await fs.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
    }
}
