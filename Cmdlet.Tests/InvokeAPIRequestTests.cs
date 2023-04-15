using OpenAICmdlet;
using Moq;
using System.Net.Http.Json;
namespace Cmdlet.Tests;

[TestClass]
public class InvokeAPIRequestTests
{
    [TestMethod]
    public void CanInvokeRequest()
    {
        var mockMsgHandler = new MockHandler((request) =>
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"Prompt\": \"hello\", \"Response\": \"World\"}"),
            };
        });

        InvokeOpenAIRequestCommand<OpenAIResponse>.httpClient = new HttpClient(mockMsgHandler);
        var mockCommand = new Mock<InvokeOpenAIRequestCommand<OpenAIResponse>>();
        mockCommand.Setup(x => x.ShouldProcess(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        mockCommand.Setup(x => x.GetAPIKey()).Returns("abcd1234");
        var invokeRequestCmd = mockCommand.Object;

        var result = invokeRequestCmd.Invoke<OpenAIResponse>();
        var response = result.First();
        Assert.IsNotNull(response);
        Assert.AreEqual(response.Prompt, "hello");
    }

    private class MockHandler : HttpMessageHandler
    {
        Func<HttpRequestMessage, HttpResponseMessage> _responseGenerator = _ => new HttpResponseMessage();
        public MockHandler(Func<HttpRequestMessage, HttpResponseMessage> responseGenerator) => _responseGenerator = responseGenerator;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = _responseGenerator(request);
            response.RequestMessage = request;
            return Task.FromResult(response);

        }
    }
}