﻿using System.Net;
using System.Net.Http.Json;

namespace OpenAI;
public static class WebRequest
{
    private static ReaderWriterLock _rwLock = new();
    private static Dictionary<string, HttpClient> _httpClientPool = new();

    public static HttpClient? GetHttpClient(string key)
    {
        try
        {
            _rwLock.AcquireReaderLock(Constant.RW_LOCK_TIMEOUT_MS);
            try
            {
                if (_httpClientPool.TryGetValue(key, out var client))
                {
                    return client;
                }
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }
        catch (ApplicationException)
        {
        }
        return null;
    }

    public static HttpClient AddHttpClient(string key, HttpMessageHandler? messageHandler = null,
                                             string? apiKey = null)
    {
        _rwLock.AcquireWriterLock(Constant.RW_LOCK_TIMEOUT_MS);
        try
        {
            var httpClient = CreateHttpClient(messageHandler, apiKey);
            _httpClientPool[key] = httpClient;
            return httpClient;
        }
        finally
        {
            _rwLock.ReleaseWriterLock();
        }
    }

    public static async Task<JsonNode?> GetAsync(
        this HttpClient client, Uri endpoint, CancellationToken cancellationToken = default) =>
        await client.GetFromJsonAsync<JsonNode>(endpoint, cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);
    public static async Task<JsonNode?> InvokeAsync(
        this HttpClient client, Uri endpoint, HttpMethod method, HttpContent content,
        CancellationToken cancellationToken = default) => method?.Method switch
        {
            WebRequestMethods.Http.Post =>
                await Task
                    .Run<JsonNode?>(
                        () =>
                        {
                            var response =
                                client.PostAsync(endpoint, content, cancellationToken).Result;
                            response.EnsureSuccessStatusCode();
                            return response.Content.ReadFromJsonAsync<JsonNode?>().Result;
                        })
                    .ConfigureAwait(continueOnCapturedContext: false),
            WebRequestMethods.Http.Put =>
                await Task
                    .Run<JsonNode?>(
                        () =>
                        {
                            var response = client.PutAsync(endpoint, content, cancellationToken).Result;
                            response.EnsureSuccessStatusCode();
                            return response.Content.ReadFromJsonAsync<JsonNode?>().Result;
                        })
                    .ConfigureAwait(continueOnCapturedContext: false),
            _ => throw new ArgumentException("Invalid HTTP Method provided!")
        };
    public static async Task<JsonNode?> InvokeAsync(
        this HttpClient client, Uri endpoint, HttpMethod method, MultipartFormDataContent content,
        CancellationToken cancellationToken = default) => method?.Method switch
        {
            WebRequestMethods.Http.Post =>
                await Task
                    .Run<JsonNode?>(
                        () =>
                        {
                            var response =
                                client.PostAsync(endpoint, content, cancellationToken).Result;
                            response.EnsureSuccessStatusCode();
                            return response.Content.ReadFromJsonAsync<JsonNode?>().Result;
                        })
                    .ConfigureAwait(continueOnCapturedContext: false),
            WebRequestMethods.Http.Put =>
                await Task
                    .Run<JsonNode?>(
                        () =>
                        {
                            var response = client.PutAsync(endpoint, content, cancellationToken).Result;
                            response.EnsureSuccessStatusCode();
                            return response.Content.ReadFromJsonAsync<JsonNode?>().Result;
                        })
                    .ConfigureAwait(continueOnCapturedContext: false),
            _ => throw new ArgumentException("Invalid HTTP Method provided!")
        };


    private static HttpClient CreateHttpClient(HttpMessageHandler? messageHandler = null,
                                               string? apiKey = null)
    {
        var client =
            (messageHandler == null)
                ? new HttpClient() { Timeout = TimeSpan.FromMinutes(Constant.HTTP_TIMEOUT_MIN) }
                : new HttpClient(
                      messageHandler)
                { Timeout = TimeSpan.FromMinutes(Constant.HTTP_TIMEOUT_MIN) };

        client.DefaultRequestHeaders.Clear();
        if (!string.IsNullOrEmpty(apiKey))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        }
        return client;
    }
}
