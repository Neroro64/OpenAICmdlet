using System.Text.Json;
using System.Net;
using System.Net.Http.Json;

namespace OpenAICmdlet;
internal static class WebRequest
{
    private static readonly int TIMEOUT_MIN = 5;
    internal static HttpClient HttpClient
    {
        get
        {
            if (_httpClient?.Value != null)
                return _httpClient.Value;
            else
                return CreateHttpClient();

        }
        set { HttpClient = value; }
    }
    private static Lazy<HttpClient> _httpClient = new(() => CreateHttpClient());
    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient() { Timeout = TimeSpan.FromMinutes(TIMEOUT_MIN) };
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }
    internal static async Task<TReturn?> GetAsync<TReturn>(
        Uri endpoint, HttpMethod method, CancellationToken cancellationToken = default)
        where TReturn : new() => await HttpClient.GetFromJsonAsync<TReturn>(endpoint, cancellationToken);
    internal static async Task<TReturn?> InvokeAsync<TReturn>(
        Uri endpoint, HttpMethod method, HttpContent? content,
        CancellationToken cancellationToken = default)
        where TReturn : new()
        => method.Method switch
        {
            WebRequestMethods.Http.Post => await Task.Run<TReturn?>(() =>
            {
                var response = HttpClient.PutAsync(endpoint, content, cancellationToken).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadFromJsonAsync<TReturn?>().Result;
            }),
            WebRequestMethods.Http.Put => await Task.Run<TReturn?>(() =>
            {
                var response = HttpClient.PostAsync(endpoint, content, cancellationToken).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadFromJsonAsync<TReturn?>().Result;
            }),
            _ => throw new ArgumentException("Invalid HTTP Method provided!")
        };
}