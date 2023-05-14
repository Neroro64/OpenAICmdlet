namespace OpenAICmdlet;

[Cmdlet(VerbsCommon.Get, "OpenAIReponseHistory")]
[OutputType(typeof(List<List<Response>>))]
public class GetOpenAIResponseHistoryCommand : MyCmdlet
{
    [Parameter()]
    [ValidateSet(nameof(TaskCategory.Text), nameof(TaskCategory.Image), nameof(TaskCategory.Audio))]
    public TaskCategory CommandCategory { get; init; } = TaskCategory.Text;

    protected override void EndProcessing()
    {
        switch (CommandCategory)
        {
            case TaskCategory.Text:
                WriteObject(InvokeOpenAITextCommand.History);
                break;
            case TaskCategory.Image:
                WriteObject(InvokeOpenAIImageCommand.History);
                break;
            case TaskCategory.Audio:
                WriteObject(InvokeOpenAIAudioCommand.History);
                break;
        }
    }
}
