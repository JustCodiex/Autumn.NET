using System.Text;
using System.Text.Json;

namespace Autumn.Http;

/// <summary>
/// Provides an abstract base class for HTTP responses, implementing the <see cref="IHttpResponseMapper"/> interface.
/// </summary>
public abstract class HttpResponse : IHttpResponseMapper {

    /// <inheritdoc/>
    public abstract string ContentType { get; }

    /// <inheritdoc/>
    public abstract Encoding ContentEncoding { get; }

    /// <inheritdoc/>
    public abstract int StatusCode { get; init; }

    /// <inheritdoc/>
    public abstract byte[] GetResponse();

    /// <summary>
    /// Creates a new <see cref="HttpResponse{T}"/> object with a status code of 200 (OK) and the specified result.
    /// </summary>
    /// <typeparam name="T">The type of the result to include in the response.</typeparam>
    /// <param name="result">The result to include in the response.</param>
    /// <returns>A new HttpResponse object.</returns>
    public static HttpResponse<T> Ok<T>(T result) => new HttpResponse<T>() { StatusCode = 200, Value = result };

    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> object with a status code of 404 (Not Found).
    /// </summary>
    /// <returns>A new HttpErrorResponse object with a 404 status code.</returns>
    public static HttpResponse NotFound() => new HttpErrorResponse<object>() { StatusCode = 404, Descriptor = "Not Found" };

    /// <summary>
    /// Creates a new <see cref="HttpResponse{T}"/> object with a status code of 404 (Not Found).
    /// </summary>
    /// <param name="message">The message to return in the response</param>
    /// <typeparam name="T">The type of the error response.</typeparam>
    /// <returns>A new HttpErrorResponse object with a 404 status code.</returns>
    public static HttpResponse<T> NotFound<T>(string message = "Not Found") => new HttpErrorResponse<T>() { StatusCode = 404, Descriptor = message };

    /// <summary>
    /// Creates a new <see cref="HttpResponse{T}"/> object with a status code of 400 (Bad Request) and an optional message.
    /// </summary>
    /// <typeparam name="T">The type of the response.</typeparam>
    /// <param name="message">The message to include in the response. Optional.</param>
    /// <returns>A new HttpResponse object with a 400 status code.</returns>
    public static HttpResponse<T> BadRequest<T>(string message = "Bad Request") => new HttpErrorResponse<T>() { StatusCode = 400, Descriptor = message };

    /// <summary>
    /// Creates a new <see cref="HttpResponse{T}"/> object with a status code of 500 (Internal Server Error) and an optional message.
    /// </summary>
    /// <param name="message">The message to include in the response. Optional.</param>
    /// <returns>A new HttpResponse object with a 500 status code.</returns>
    public static HttpResponse InternalServerError(string message = "Internal Server Error") => new HttpErrorResponse<object>() { StatusCode = 500, Descriptor = message };

    /// <summary>
    /// Creates a new <see cref="HttpResponse{T}"/> object with a status code of 500 (Internal Server Error) and an optional message.
    /// </summary>
    /// <typeparam name="T">The type of the response.</typeparam>
    /// <param name="message">The message to include in the response. Optional.</param>
    /// <returns>A new HttpResponse object with a 500 status code.</returns>
    public static HttpResponse<T> InternalServerError<T>(string message = "Internal Server Error") => new HttpErrorResponse<T>() { StatusCode = 500, Descriptor = message };

}

/// <summary>
/// Represents a concrete HTTP response with a specific type of result.
/// </summary>
/// <typeparam name="T">The type of the result included in the response.</typeparam>
public class HttpResponse<T> : HttpResponse {

    /// <inheritdoc/>
    public override string ContentType => this.Value is not null ? "text/json; charset='utf-8'" : "text/plain";

    /// <inheritdoc/>
    public override Encoding ContentEncoding => Encoding.UTF8;

    /// <inheritdoc/>
    public override int StatusCode { get; init; }

    /// <summary>
    /// Gets or sets the value of the response.
    /// </summary>
    public T? Value { get; set; }

    /// <inheritdoc/>
    public override byte[] GetResponse() => Value switch {
        string s => ContentEncoding.GetBytes(s),
        null => Array.Empty<byte>(),
        _ => ContentEncoding.GetBytes(JsonSerializer.Serialize(Value)),
    };

}

/// <summary>
/// Represents a concrete HTTP error response with a specific message.
/// </summary>
/// <typeparam name="T">The type of the error response.</typeparam>
public sealed class HttpErrorResponse<T> : HttpResponse<T> {

    /// <summary>
    /// Gets or initializes the descriptive message for the error response.
    /// </summary>
    public string Descriptor { get; init; } = string.Empty;

    /// <inheritdoc/>
    public override byte[] GetResponse() => this.ContentEncoding.GetBytes(Descriptor);

}
