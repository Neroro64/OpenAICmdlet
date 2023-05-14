namespace OpenAICmdlet;

public class MyOpenAICmdlet : MyCmdlet
{

    private static History _history { get; set; } = new();
    public static History History => _history;
    public static void RestoreHistory(History previousHistory, bool force)
    {
        if (!force && _history.Count > 0)
        {
            throw new InvalidOperationException("The history in the current session is not empty. Use -Force to override it.");
        }
        _history = previousHistory;
    }
}
