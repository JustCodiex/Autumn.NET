using System.Net;

namespace Autumn.Http.Sessions;

/// <summary>
/// Manages HTTP sessions, providing methods to retrieve, check, and destroy sessions.
/// </summary>
public interface IHttpSessionManager {

    /// <summary>
    /// Retrieves the HTTP session associated with the specified context and query parameters.
    /// </summary>
    /// <param name="context">The <see cref="HttpListenerContext"/> for the current request.</param>
    /// <param name="queryParameters">A dictionary of query parameters.</param>
    /// <returns>The corresponding <see cref="IHttpSession"/> if it exists; otherwise, null.</returns>
    IHttpSession? GetSession(HttpListenerContext context, IDictionary<string, string> queryParameters);

    /// <summary>
    /// Determines whether the specified session is expired.
    /// </summary>
    /// <param name="session">The session to check.</param>
    /// <returns>True if the session is expired; otherwise, false.</returns>
    bool IsExpired(IHttpSession session);

    /// <summary>
    /// Destroys all sessions that are inactive and considered expired.
    /// </summary>
    void DestroyInactiveSessions();

}
