using OpenAICmdlet;
using Moq;
namespace Cmdlet.Tests;

[TestClass]
public class APIKeyTests
{
    public static readonly string localAPIKeyPath = System.IO.Path.Join(
          Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
          "OpenAICmdlet/API.key");

    [ClassCleanup]
    public static void ClearLocalFiles()
    {
        if (File.Exists(localAPIKeyPath))
        {
            File.Delete(localAPIKeyPath);
            if (Directory.GetParent(localAPIKeyPath)?.FullName is { } parentDir)
                Directory.Delete(parentDir);
        }
    }

    [TestMethod]
    public void CanSetAndGetAPIKey()
    {
        var apiKey = "abcd1234";
        var mock = new Mock<SetOpenAIAPIKeyCommand>() { CallBase = true };
        mock.Setup(x => x.ReadConsoleLine(It.IsAny<string>())).Returns(apiKey).Verifiable();
        mock.Setup(x => x.ShouldProcess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        var setKeyCmd = mock.Object;
        var returnedObjCount = setKeyCmd.Invoke<object>().Count();
        Assert.AreEqual(returnedObjCount, 0);
        Assert.IsTrue(File.Exists(localAPIKeyPath));
        mock.Verify();

        var getKeyCmd = new GetOpenAIAPIKeyCommand();
        var res = getKeyCmd.Invoke<string>().First();
        Assert.IsNotNull(res, apiKey);
    }
}
