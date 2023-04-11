namespace OpenAICmdlet;

public class MyCmdlet : Cmdlet
{
    public virtual string ReadConsoleLine(string prompt)
    {
        System.Console.WriteLine(prompt);
        return System.Console.ReadLine() ?? "";
    }
    public virtual new bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
    {
        return base.ShouldProcess(verboseDescription, verboseWarning, caption);
    }
    public virtual void TestInvoke()
    {
        BeginProcessing();
        ProcessRecord();
        EndProcessing();
    }
}