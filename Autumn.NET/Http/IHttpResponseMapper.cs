using System.Text;

namespace Autumn.Http;

/// <summary>
/// Defines a contract for mapping HTTP response data, including content type, content encoding, status code, and the response body.
/// </summary>
public interface IHttpResponseMapper {

    /// <summary>
    /// Gets the content type of the HTTP response.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Gets the content encoding of the HTTP response.
    /// </summary>
    Encoding ContentEncoding { get; }

    /// <summary>
    /// Gets or sets the status code of the HTTP response.
    /// </summary>
    int StatusCode { get; }

    /// <summary>
    /// Gets the response body as a byte array.
    /// </summary>
    /// <returns>The HTTP response body as a byte array.</returns>
    byte[] GetResponse();

}
