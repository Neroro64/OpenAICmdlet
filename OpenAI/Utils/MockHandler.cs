namespace OpenAI;
public class MockHandler : HttpMessageHandler
{
    Func<HttpRequestMessage, HttpResponseMessage> _responseGenerator =
        _ => new HttpResponseMessage();
    public MockHandler(Func<HttpRequestMessage, HttpResponseMessage> responseGenerator) =>
        _responseGenerator = responseGenerator;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                           CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = _responseGenerator(request);
        response.RequestMessage = request;
        return Task.FromResult(response);
    }
}
