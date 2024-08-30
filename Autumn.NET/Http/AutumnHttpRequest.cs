using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace Autumn.Http;

/// <summary>
/// Sealed class representing a basic HTTP request. Implements <see cref="IHttpRequest"/>.
/// </summary>
public sealed class AutumnHttpRequest : IHttpRequest, IDisposable {

    /// <inheritdoc/>
    public string HttpMethod { get; }
    /// <inheritdoc/>
    public Uri Url { get; }
    /// <inheritdoc/>
    public string RawUrl { get; }
    /// <inheritdoc/>
    public NameValueCollection Headers { get; }
    /// <inheritdoc/>
    public string ContentType { get; }
    /// <inheritdoc/>
    public long ContentLength64 { get; }
    /// <inheritdoc/>
    public Encoding ContentEncoding { get; }
    /// <inheritdoc/>
    public string UserAgent { get; }
    /// <inheritdoc/>
    public string[] AcceptTypes { get; }
    /// <inheritdoc/>
    public Stream BodyStream { get; }
    /// <inheritdoc/>
    public bool HasEntityBody { get; }

    /// <summary>
    /// Initialize a new <see cref="AutumnHttpRequest"/> based on the base <see cref="HttpListenerRequest"/>.
    /// </summary>
    /// <param name="request">Listener request to copy data from</param>
    public AutumnHttpRequest(HttpListenerRequest request) {

        // Copy basic properties
        HttpMethod = request.HttpMethod;
        Url = request.Url!;
        RawUrl = request.RawUrl!;
        ContentType = request.ContentType!;
        ContentLength64 = request.ContentLength64;
        ContentEncoding = request.ContentEncoding;
        UserAgent = request.UserAgent;
        AcceptTypes = request.AcceptTypes!;
        HasEntityBody = request.HasEntityBody;

        // Copy headers
        Headers = new NameValueCollection(request.Headers);

        // Copy body stream to a memory stream
        if (request.InputStream != null) {
            BodyStream = new MemoryStream();
            request.InputStream.CopyTo(BodyStream);
            BodyStream.Position = 0; // Reset the memory stream position to the beginning
        } else {
            BodyStream = new MemoryStream();
        }

    }

    /// <inheritdoc/>
    public string GetBodyAsString() {
        if (BodyStream == null)
            return string.Empty;

        // Ensure the stream is at the beginning
        BodyStream.Position = 0;

        using (StreamReader reader = new StreamReader(BodyStream, ContentEncoding ?? Encoding.UTF8, true, 1024, true)) {
            return reader.ReadToEnd();
        }
    }

    /// <inheritdoc/>
    public void ResetBodyStreamPosition() {
        if (BodyStream != null) {
            BodyStream.Position = 0;
        }
    }

    /// <summary>
    /// Disposes stored HTTP request data
    /// </summary>
    public void Dispose() {
        BodyStream?.Dispose();
    }

}
