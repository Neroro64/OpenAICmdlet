using System.Text;
namespace OpenAICmdlet;

internal static class PromptBuilder
{
    internal static string BuildPrompt(string prompt, string? contextFilePath)
    {
        StringBuilder promptBuilder = new();
        if (contextFilePath != null && File.Exists(contextFilePath))
        {
            // TODO: Add a layer of cleaning, normalizing and summarizing if needed 
            using (FileStream fs = File.OpenRead(contextFilePath))
            using (StreamReader reader = new(fs))
            {
                promptBuilder.Append(reader.ReadToEnd());
            }
        }
        promptBuilder.AppendLine(prompt);
        return promptBuilder.ToString();
    }

    internal static List<Dictionary<string, string>> BuildChat(
        string initMessage,
        string prompt,
        string? contextFilePath,
        bool prependHistory,
        List<OpenAIResponse> history)
    {
        var messages = new List<Dictionary<string, string>>()
        {
            new()
            {
                ["role"] = "system",
                ["content"] = initMessage,
            }
        };
        if (prependHistory)
        {
            foreach (var response in history)
            {
                messages.Add(new Dictionary<string, string>() { ["role"] = "user", ["content"] = response.Prompt });
                messages.Add(new Dictionary<string, string>() { ["role"] = "assistant", ["content"] = response.Response });
            }
        }
        messages.Add(new Dictionary<string, string>() { ["role"] = "user", ["content"] = prompt });
        return messages;
    }
}