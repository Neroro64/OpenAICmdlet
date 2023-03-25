﻿using System.Management.Automation;

namespace OpenAICmdlet;

[Cmdlet(
    VerbCommon.Invoke, "OpenAI",
    SupportsShouldProcess,
    DefaultParameterSetName = "Text")]
[OutputType(typeof(OpenAIResponse))]
public class InvokeOpenAI : Cmdlet
{
    [Parameter(ParameterSetName = "Text")]
    [Parameter(ParameterSetName = "Chat")]
    [Parameter(ParameterSetName = "Image")]
    [Parameter(ParameterSetName = "Audio")]
    [Parameter(ParameterSetName = "Embedding")]
    [Parameter(
        Mandatory = true,
        Position = 0,
        HelpMessage = "Desired task to perform")]
    public OpenAITaskEnum Task { get; set; } = OpenAITaskEnum.Text;

    [Parameter(
        Mandatory = true,
        Position = 1,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    public string Prompt { get; init; }

    // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
    protected override void BeginProcessing()
    {
        WriteVerbose("Begin!");
    }

    // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
    protected override void ProcessRecord()
    {
        WriteObject(new FavoriteStuff { 
            FavoriteNumber = FavoriteNumber,
            FavoritePet = FavoritePet
        });
    }

    // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
    protected override void EndProcessing()
    {
        WriteVerbose("End!");
    }
}

public class FavoriteStuff
{
    public int FavoriteNumber { get; set; }
    public string FavoritePet { get; set; }
}
