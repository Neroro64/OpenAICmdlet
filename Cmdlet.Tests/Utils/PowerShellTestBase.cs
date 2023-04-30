using System.Management.Automation;
using System.Management.Automation.Runspaces;
namespace OpenAICmdlet.Tests;

public static class PowerShellTestBase 
{
    public static PowerShell CreatePowerShell(string commandName, Type commandType)
    {
        var initialSessionState = InitialSessionState.CreateDefault();
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            commandName, commandType, null));
        
        return PowerShell.Create(initialSessionState);
    }
}

