using System.Management.Automation;
using System.Management.Automation.Runspaces;
namespace OpenAICmdlet.Tests;

public static class PowerShellTestBase 
{
    public static PowerShell CreatePowerShell(string commandName, Type commandType)
    {
        return CreatePowerShell(new[] { commandName }, new[] { commandType });
    }
    public static PowerShell CreatePowerShell(string[] commandNames, Type[] commandTypes)
    {
        var initialSessionState = InitialSessionState.CreateDefault();
        foreach ((var commandName, var commandType) in commandNames.Zip(commandTypes))
        {
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                commandName, commandType, null));
        }
        return PowerShell.Create(initialSessionState);
    }
}

