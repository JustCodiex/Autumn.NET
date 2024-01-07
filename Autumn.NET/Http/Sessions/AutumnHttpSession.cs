using System.Net;

using Autumn.Context;

namespace Autumn.Http.Sessions;

/// <summary>
/// Represents a concrete implementation of IHttpSession providing mechanisms for session management and value storage.
/// </summary>
public sealed class AutumnHttpSession : IHttpSession {

    private readonly Dictionary<string, object?> values;
    private DateTime expiresAt;

    /// <inheritdoc/>
    public event ScopeContextDestroyedHandler? OnContextDestroyed;

    /// <inheritdoc/>
    public string SessionIdentifier { get; }

    /// <inheritdoc/>
    public DateTime ValidFrom { get; }

    /// <inheritdoc/>
    public DateTime ValidTo => this.expiresAt;

    /// <summary>
    /// Initializes a new instance of the AutumnHttpSession with the specified identifier, valid from and to dates.
    /// </summary>
    /// <param name="identifier">The unique identifier for the session.</param>
    /// <param name="validFrom">The start date and time of the session's validity.</param>
    /// <param name="validTo">The end date and time of the session's validity.</param>
    public AutumnHttpSession(string identifier, DateTime validFrom, DateTime validTo) {
        this.SessionIdentifier = identifier;
        this.ValidFrom = validFrom;
        this.expiresAt = validTo;
        this.values = new();
    }

    /// <inheritdoc/>
    public void StoreSessionValue<T>(string key, T value) => values[key] = value;

    /// <inheritdoc/>
    public T? GetSessionValue<T>(string key) {
        if (this.values.TryGetValue(key, out object? val) && val is T t) {
            return t;
        }
        return default;
    }

    /// <inheritdoc/>
    public void Destroy() {
        OnContextDestroyed?.Invoke(this);
        values.Clear();
    }

    /// <inheritdoc/>
    public IHttpSession ExtendLifeSpan(TimeSpan life) {
        this.expiresAt += life;
        return this;
    }

    /// <inheritdoc/>
    public Cookie ToCookie(string name, string domain)
        => new Cookie(name, this.SessionIdentifier, "/", domain) {
            Expires = this.expiresAt,
            HttpOnly = true,
        };

}
