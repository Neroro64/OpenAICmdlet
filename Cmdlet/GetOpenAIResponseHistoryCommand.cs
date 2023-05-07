namespace OpenAICmdlet;

[Cmdlet(VerbsCommon.Get, "OpenAIReponseHistory")]
[OutputType(typeof(List<List<OpenAIResponse>>))]
public class GetOpenAIResponseHistoryCommand : MyCmdlet
{
    [Parameter()]
    [ValidateSet(nameof(OpenAICategory.Text), nameof(OpenAICategory.Image), nameof(OpenAICategory.Audio))]
    public OpenAICategory CommandCategory { get; init; } = OpenAICategory.Text;

    protected override void EndProcessing()
    {
        switch (CommandCategory)
        {
            case OpenAICategory.Text:
                WriteObject(InvokeOpenAITextCommand.History);
                break;
            case OpenAICategory.Image:
                WriteObject(InvokeOpenAIImageCommand.History);
                break;
            case OpenAICategory.Audio:
                WriteObject(InvokeOpenAIAudioCommand.History);
                break;
        }
    }
}
