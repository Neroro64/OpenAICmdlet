using System.Runtime.Serialization;
namespace OpenAICmdlet.Tests;

[Serializable]
public class MockRequestBody
{
    public string Name { get; set; } = "Test";
    public int Value { get; set; } = 1234;
    public List<string> ListOfStrings { get; set; } = new() { "Hello", "World" };
    public Dictionary<int, string> DictOfStrings { get; set; } = new() {
        {1, "Hello"},
        {2, "World"}
    };
}