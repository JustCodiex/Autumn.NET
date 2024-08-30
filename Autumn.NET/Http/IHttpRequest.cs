using System.Collections.Specialized;
using System.Text;

namespace Autumn.Http;

/// <summary>
/// Interface representing a basic HTTP request.
/// </summary>
public interface IHttpRequest {

    /// <summary>
    /// Gets the HTTP method specified by the client.
    /// </summary>
    string HttpMethod { get; }

    /// <summary>
    /// Gets the <see cref="Uri"/> object requested by the client.
    /// </summary>
    Uri Url { get; }

    /// <summary>
    /// Gets the URL information (Without the Host and Port) requested by the client.
    /// </summary>
    string RawUrl { get; }

    /// <summary>
    /// Gets the collection of Header name/value pairs sent in the request.
    /// </summary>
    NameValueCollection Headers { get; }

    /// <summary>
    /// Gets the content type specified by the client
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Gets the content length specified by the client
    /// </summary>
    long ContentLength64 { get; }

    /// <summary>
    /// Gets the content encoding specified by the client
    /// </summary>
    Encoding ContentEncoding { get; }

    /// <summary>
    /// Gets the user-agent specified by the client
    /// </summary>
    string UserAgent { get; }

    /// <summary>
    /// Gets the MIME types accepted by the client
    /// </summary>
    string[] AcceptTypes { get; }

    /// <summary>
    /// Gets the underlying <see cref="Stream"/> containing the body data.
    /// </summary>
    /// <remarks>
    /// Not thread-safe
    /// </remarks>
    Stream BodyStream { get; }

    /// <summary>
    /// Gets a bool indicating whether the request has body data.
    /// </summary>
    bool HasEntityBody { get; }

    /// <summary>
    /// Method to read the body as a string
    /// </summary>
    /// <returns>The string contents of the body</returns>
    string GetBodyAsString();

    /// <summary>
    /// Method to reset the stream position for further reads
    /// </summary>
    void ResetBodyStreamPosition();

}
