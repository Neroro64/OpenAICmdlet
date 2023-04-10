using System.Management.Automation;

namespace OpenAICmdlet;

[Cmdlet(VerbsLifecycle.Invoke, "OpenAIText", SupportsShouldProcess = true)]
[Alias("ai")]
[OutputType(typeof(OpenAIResponse))]
public class InvokeOpenAITextCommand : Cmdlet {
  [Parameter(
      Mandatory = true, Position = 0, ValueFromPipeline = true,
      ValueFromPipelineByPropertyName = true,
      HelpMessage =
          "The prompt(s) to generate completions for, encoded as a string")]
  public string Prompt { get; set; } = "";

  protected override void BeginProcessing() { WriteVerbose("Begin!"); }

  protected override void ProcessRecord() { WriteObject(Prompt); }

  protected override void EndProcessing() { WriteVerbose("End!"); }
}
