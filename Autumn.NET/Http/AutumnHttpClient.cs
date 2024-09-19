using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

using Autumn.Annotations;

namespace Autumn.Http;

/// <summary>
/// 
/// </summary>
/// <param name="httpClient"></param>
[Component]
public sealed class AutumnHttpClient([Inject] HttpClient httpClient) : IHttpClient {

    /// <inheritdoc/>
    public HttpRequestHeaders DefaultRequestHeaders => httpClient.DefaultRequestHeaders;

    /// <inheritdoc/>
    public Version DefaultRequestVersion {
        get => httpClient.DefaultRequestVersion;
        set => httpClient.DefaultRequestVersion = value;
    }

    /// <inheritdoc/>
    public HttpVersionPolicy DefaultVersionPolicy {
        get => httpClient.DefaultVersionPolicy;
        set => httpClient.DefaultVersionPolicy = value;
    }

    /// <inheritdoc/>
    public Uri? BaseAddress { 
        get => httpClient.BaseAddress;
        set => httpClient.BaseAddress = value;
    }

    /// <inheritdoc/>
    public TimeSpan Timeout { 
        get => httpClient.Timeout;
        set => httpClient.Timeout = value;
    }

    /// <inheritdoc/>
    public long MaxResponseContentBufferSize {
        get => httpClient.MaxResponseContentBufferSize;
        set => httpClient.MaxResponseContentBufferSize = value;
    }

    /// <inheritdoc/>
    public void CancelPendingRequests() => httpClient.CancelPendingRequests();

    /// <inheritdoc/>
    public Task<HttpResponseMessage> DeleteAsync([StringSyntax("Uri")] string? requestUri) => httpClient.DeleteAsync(requestUri);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> DeleteAsync(Uri? requestUri) => httpClient.DeleteAsync(requestUri);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> DeleteAsync([StringSyntax("Uri")] string? requestUri, CancellationToken cancellationToken) => httpClient.DeleteAsync(requestUri, cancellationToken);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> DeleteAsync(Uri? requestUri, CancellationToken cancellationToken) => httpClient.DeleteAsync(requestUri, cancellationToken);

    /// <inheritdoc/>
    public void Dispose() => httpClient.Dispose();

    /// <inheritdoc/>
    public Task<HttpResponseMessage> GetAsync([StringSyntax("Uri")] string? requestUri) => httpClient.GetAsync(requestUri);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> GetAsync(Uri? requestUri) => httpClient.GetAsync(requestUri);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> GetAsync([StringSyntax("Uri")] string? requestUri, HttpCompletionOption completionOption) => httpClient.GetAsync(requestUri, completionOption);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> GetAsync(Uri? requestUri, HttpCompletionOption completionOption) => httpClient.GetAsync(requestUri, completionOption);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> GetAsync([StringSyntax("Uri")] string? requestUri, CancellationToken cancellationToken) => httpClient.GetAsync(requestUri, cancellationToken);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> GetAsync(Uri? requestUri, CancellationToken cancellationToken) => httpClient.GetAsync(requestUri, cancellationToken);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> GetAsync([StringSyntax("Uri")] string? requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken) => httpClient.GetAsync(requestUri, completionOption, cancellationToken);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> GetAsync(Uri? requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken) => httpClient.GetAsync(requestUri, completionOption, cancellationToken);

    /// <inheritdoc/>
    public Task<byte[]> GetByteArrayAsync([StringSyntax("Uri")] string? requestUri) => httpClient.GetByteArrayAsync(requestUri);

    /// <inheritdoc/>
    public Task<byte[]> GetByteArrayAsync(Uri? requestUri) => httpClient.GetByteArrayAsync(requestUri);

    /// <inheritdoc/>
    public Task<byte[]> GetByteArrayAsync([StringSyntax("Uri")] string? requestUri, CancellationToken cancellationToken) => httpClient.GetByteArrayAsync(requestUri, cancellationToken);

    /// <inheritdoc/>
    public Task<byte[]> GetByteArrayAsync(Uri? requestUri, CancellationToken cancellationToken) => httpClient.GetByteArrayAsync(requestUri, cancellationToken);

    /// <inheritdoc/>
    public Task<Stream> GetStreamAsync([StringSyntax("Uri")] string? requestUri) => httpClient.GetStreamAsync(requestUri);

    /// <inheritdoc/>
    public Task<Stream> GetStreamAsync([StringSyntax("Uri")] string? requestUri, CancellationToken cancellationToken) => httpClient.GetStreamAsync(requestUri, cancellationToken);

    /// <inheritdoc/>
    public Task<Stream> GetStreamAsync(Uri? requestUri) => httpClient.GetStreamAsync(requestUri);

    /// <inheritdoc/>
    public Task<Stream> GetStreamAsync(Uri? requestUri, CancellationToken cancellationToken) => httpClient.GetStreamAsync(requestUri, cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetStringAsync([StringSyntax("Uri")] string? requestUri) => httpClient.GetStringAsync(requestUri);

    /// <inheritdoc/>
    public Task<string> GetStringAsync(Uri? requestUri) => httpClient.GetStringAsync(requestUri);

    /// <inheritdoc/>
    public Task<string> GetStringAsync([StringSyntax("Uri")] string? requestUri, CancellationToken cancellationToken) => httpClient.GetStringAsync(requestUri, cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetStringAsync(Uri? requestUri, CancellationToken cancellationToken) => httpClient.GetStringAsync(requestUri, cancellationToken);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> PatchAsync([StringSyntax("Uri")] string? requestUri, HttpContent? content) => httpClient.PatchAsync(requestUri, content);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> PatchAsync(Uri? requestUri, HttpContent? content) => httpClient.PatchAsync(requestUri, content);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> PatchAsync([StringSyntax("Uri")] string? requestUri, HttpContent? content, CancellationToken cancellationToken) => httpClient.PatchAsync(requestUri, content, cancellationToken);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> PatchAsync(Uri? requestUri, HttpContent? content, CancellationToken cancellationToken) => httpClient.PatchAsync(requestUri, content, cancellationToken);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> PostAsync([StringSyntax("Uri")] string? requestUri, HttpContent? content) => httpClient.PostAsync(requestUri, content);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> PostAsync(Uri? requestUri, HttpContent? content) => httpClient.PostAsync(requestUri, content);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> PostAsync([StringSyntax("Uri")] string? requestUri, HttpContent? content, CancellationToken cancellationToken) => httpClient.PostAsync(requestUri, content, cancellationToken);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> PostAsync(Uri? requestUri, HttpContent? content, CancellationToken cancellationToken) => httpClient.PostAsync(requestUri, content, cancellationToken);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> PutAsync([StringSyntax("Uri")] string? requestUri, HttpContent? content) => httpClient.PutAsync(requestUri, content);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> PutAsync(Uri? requestUri, HttpContent? content) => httpClient.PutAsync(requestUri, content);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> PutAsync([StringSyntax("Uri")] string? requestUri, HttpContent? content, CancellationToken cancellationToken) => httpClient.PutAsync(requestUri, content, cancellationToken);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> PutAsync(Uri? requestUri, HttpContent? content, CancellationToken cancellationToken) => httpClient.PutAsync(requestUri, content, cancellationToken);

    /// <inheritdoc/>
    public HttpResponseMessage Send(HttpRequestMessage request) => httpClient.Send(request);

    /// <inheritdoc/>
    public HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completionOption) => httpClient.Send(request, completionOption);

    /// <inheritdoc/>
    public HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) => httpClient.Send(request, cancellationToken);

    /// <inheritdoc/>
    public HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken) => httpClient.Send(request, completionOption, cancellationToken);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken) => httpClient.SendAsync(request, completionOption, cancellationToken);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request) => httpClient.SendAsync(request);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => httpClient.SendAsync(request, cancellationToken);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption) => httpClient.SendAsync(request, completionOption);

}
