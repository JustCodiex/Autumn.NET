using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Runtime.Versioning;

namespace Autumn.Http;

public interface IHttpClient : IDisposable {

    #region .NET HttpClient methods

    HttpRequestHeaders DefaultRequestHeaders { get; }
    Version DefaultRequestVersion { get; set; }
    HttpVersionPolicy DefaultVersionPolicy { get; set; }
    Uri? BaseAddress { get; set; }
    TimeSpan Timeout { get; set; }
    long MaxResponseContentBufferSize { get; set; }

    Task<string> GetStringAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri);
    Task<string> GetStringAsync(Uri? requestUri);
    Task<string> GetStringAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, CancellationToken cancellationToken);
    Task<string> GetStringAsync(Uri? requestUri, CancellationToken cancellationToken);

    Task<byte[]> GetByteArrayAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri);
    Task<byte[]> GetByteArrayAsync(Uri? requestUri);
    Task<byte[]> GetByteArrayAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, CancellationToken cancellationToken);
    Task<byte[]> GetByteArrayAsync(Uri? requestUri, CancellationToken cancellationToken);

    Task<Stream> GetStreamAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri);
    Task<Stream> GetStreamAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, CancellationToken cancellationToken);
    Task<Stream> GetStreamAsync(Uri? requestUri);
    Task<Stream> GetStreamAsync(Uri? requestUri, CancellationToken cancellationToken);

    Task<HttpResponseMessage> GetAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri);
    Task<HttpResponseMessage> GetAsync(Uri? requestUri);
    Task<HttpResponseMessage> GetAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpCompletionOption completionOption);
    Task<HttpResponseMessage> GetAsync(Uri? requestUri, HttpCompletionOption completionOption);
    Task<HttpResponseMessage> GetAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, CancellationToken cancellationToken);
    Task<HttpResponseMessage> GetAsync(Uri? requestUri, CancellationToken cancellationToken);
    Task<HttpResponseMessage> GetAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken);
    Task<HttpResponseMessage> GetAsync(Uri? requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken);

    Task<HttpResponseMessage> PostAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpContent? content);
    Task<HttpResponseMessage> PostAsync(Uri? requestUri, HttpContent? content);
    Task<HttpResponseMessage> PostAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpContent? content, CancellationToken cancellationToken);
    Task<HttpResponseMessage> PostAsync(Uri? requestUri, HttpContent? content, CancellationToken cancellationToken);

    Task<HttpResponseMessage> PutAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpContent? content);
    Task<HttpResponseMessage> PutAsync(Uri? requestUri, HttpContent? content);
    Task<HttpResponseMessage> PutAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpContent? content, CancellationToken cancellationToken);
    Task<HttpResponseMessage> PutAsync(Uri? requestUri, HttpContent? content, CancellationToken cancellationToken);

    Task<HttpResponseMessage> PatchAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpContent? content);
    Task<HttpResponseMessage> PatchAsync(Uri? requestUri, HttpContent? content);
    Task<HttpResponseMessage> PatchAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpContent? content, CancellationToken cancellationToken);
    Task<HttpResponseMessage> PatchAsync(Uri? requestUri, HttpContent? content, CancellationToken cancellationToken);

    Task<HttpResponseMessage> DeleteAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri);
    Task<HttpResponseMessage> DeleteAsync(Uri? requestUri);
    Task<HttpResponseMessage> DeleteAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, CancellationToken cancellationToken);
    Task<HttpResponseMessage> DeleteAsync(Uri? requestUri, CancellationToken cancellationToken);

    [UnsupportedOSPlatform("browser")] HttpResponseMessage Send(HttpRequestMessage request);
    [UnsupportedOSPlatform("browser")] HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completionOption);
    [UnsupportedOSPlatform("browser")] HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken);
    [UnsupportedOSPlatform("browser")] HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken);

    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption);
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken);

    void CancelPendingRequests();

    #endregion

    #region Autumn HttpClient methods

    // TODO: Implement

    #endregion

}
