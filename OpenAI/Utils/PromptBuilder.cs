using System.Text;
namespace OpenAI;

public static class PromptBuilder
{
    public static string BuildPrompt(string prompt, string? contextFilePath)
    {
        StringBuilder promptBuilder = new();
        if (contextFilePath != null && File.Exists(contextFilePath))
        {
            // TODO: Add a layer of cleaning, normalizing and summarizing if needed
            using (FileStream fs = File.OpenRead(contextFilePath)) using (StreamReader reader =
                                                                              new(fs))
            {
                promptBuilder.Append(reader.ReadToEnd());
            }
        }
        promptBuilder.AppendLine(prompt);
        return promptBuilder.ToString();
    }

    public static ICollection<Dictionary<string, string>> BuildChat(string initMessage, string prompt,
                                                               string? contextFilePath,
                                                               ICollection<Response>? history)
    {
        var messages = new List<Dictionary<string, string>>() { new() {
            ["role"] = "system",
            ["content"] = initMessage,
        } };
        if (history != null)
        {
            foreach (var response in history)
            {
                messages.Add(new Dictionary<string, string>()
                {
                    ["role"] = "user",
                    ["content"] = response.Prompt
                });
                messages.Add(
                    new Dictionary<string, string>()
                    {
                        ["role"] = "assistant",
                        ["content"] = response.Body.First()
                    });
            }
        }
        messages.Add(new Dictionary<string, string>() { ["role"] = "user", ["content"] = prompt });
        return messages;
    }
}
