using System.Net;

using Autumn.Context.Configuration;

namespace Autumn.Http.Sessions;

/// <summary>
/// Implements session management for HTTP requests, including retrieving, validating, and destroying sessions.
/// </summary>
public class AutumnHttpSessionManager : IHttpSessionManager {

    private readonly bool tokenIsQueryParameter;
    private readonly string tokenName;

    private readonly Dictionary<string, IHttpSession> sessions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutumnHttpSessionManager"/> with specified static properties.
    /// </summary>
    /// <param name="staticPropertySource">The static property source containing configuration settings.</param>
    public AutumnHttpSessionManager(StaticPropertySource staticPropertySource) {
        this.sessions = new();
        if (staticPropertySource.GetValueOrDefault("autumn.http.session.management.token-type", "query-param")?.ToLowerInvariant() is "query-param") {
            this.tokenIsQueryParameter = true;
        }
        this.tokenName = staticPropertySource.GetValueOrDefault("autumn.http.session.management.token-name", "_s") ?? "_s";
    }

    /// <inheritdoc/>
    public IHttpSession? GetSession(HttpListenerContext context, IDictionary<string, string> queryParameters) {
        string domain = context.Request.Url!.Host;
        string tokenIdentifier = tokenIsQueryParameter switch {
            true => queryParameters.TryGetValue(tokenName, out string? token) ? token! : string.Empty,
            false => TryGetCookieToken(domain, context.Request.Cookies)
        };
        if (string.IsNullOrEmpty(tokenIdentifier)) {
            return this.RegisterSession();
        }
        IHttpSession? activeSession = sessions.TryGetValue(tokenIdentifier, out IHttpSession? session) ? session : null;
        if (activeSession is null || IsExpired(activeSession)) {
            return null;
        }
        IHttpSession updatedSession = activeSession.ExtendLifeSpan(TimeSpan.FromMinutes(2.5)); // Keep extending lifespan
        if (!tokenIsQueryParameter) {
            context.Response.SetCookie(updatedSession.ToCookie(tokenName, domain));
        }
        return updatedSession;
    }

    /// <summary>
    /// Attempts to retrieve the session token from the cookie collection.
    /// </summary>
    /// <param name="domain">The domain from which to retrieve the cookie.</param>
    /// <param name="cookies">The collection of cookies from the HTTP request.</param>
    /// <returns>The session token if found; otherwise, an empty string.</returns>
    private string TryGetCookieToken(string domain, CookieCollection cookies) {
        Cookie? cookie = cookies.FirstOrDefault(x => x.Name == tokenName && x.Domain == domain && !x.Expired);
        if (cookie is null) {
            return string.Empty;
        }
        return cookie.Value;
    }

    /// <inheritdoc/>
    public bool IsExpired(IHttpSession session)
        => session.ValidTo.ToUniversalTime() < DateTime.UtcNow;

    /// <summary>
    /// Registers a new session and adds it to the session dictionary.
    /// </summary>
    /// <returns>The newly created <see cref="IHttpSession"/>.</returns>
    private IHttpSession RegisterSession() {
        Guid guid = Guid.NewGuid();
        return sessions[guid.ToString()] = new AutumnHttpSession(guid.ToString(), DateTime.UtcNow, DateTime.UtcNow.AddMinutes(30));
    }

    /// <inheritdoc/>
    public void DestroyInactiveSessions() {
        foreach (var session in sessions.Values.Where(IsExpired)) {
            session.Destroy();
            sessions.Remove(session.SessionIdentifier);
        }
    }

}
