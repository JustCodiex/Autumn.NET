using System.Net;

using Autumn.Context;

namespace Autumn.Http.Sessions;

/// <summary>
/// Represents a session within the HTTP context, providing methods to manage session lifecycle and store session values.
/// </summary>
public interface IHttpSession : IScopeContext {

    /// <summary>
    /// Gets the unique identifier for the session.
    /// </summary>
    string SessionIdentifier { get; }

    /// <summary>
    /// Gets the date and time when the session becomes valid.
    /// </summary>
    DateTime ValidFrom { get; }

    /// <summary>
    /// Gets the date and time when the session expires.
    /// </summary>
    DateTime ValidTo { get; }

    /// <summary>
    /// Stores a value in the session using the specified key.
    /// </summary>
    /// <typeparam name="T">The type of value to store in the session.</typeparam>
    /// <param name="key">The key associated with the value to store.</param>
    /// <param name="value">The value to store in the session.</param>
    void StoreSessionValue<T>(string key, T value);

    /// <summary>
    /// Retrieves a value from the session using the specified key.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve from the session.</typeparam>
    /// <param name="key">The key associated with the value to retrieve.</param>
    /// <returns>The value retrieved from the session if found; otherwise, the default value for the type.</returns>
    T? GetSessionValue<T>(string key);

    /// <summary>
    /// Extends the session's lifespan by the specified duration.
    /// </summary>
    /// <param name="life">The duration to add to the session's current lifespan.</param>
    /// <returns>The updated IHttpSession with an extended lifespan.</returns>
    IHttpSession ExtendLifeSpan(TimeSpan life);

    /// <summary>
    /// Converts the session into a cookie with the specified name and domain.
    /// </summary>
    /// <param name="name">The name of the cookie.</param>
    /// <param name="domain">The domain to associate with the cookie.</param>
    /// <returns>A Cookie object representing the session.</returns>
    Cookie ToCookie(string name, string domain);

}
