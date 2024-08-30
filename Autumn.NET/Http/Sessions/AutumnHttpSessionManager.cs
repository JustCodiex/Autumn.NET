using System.Net;

using Autumn.Annotations;
using Autumn.Context.Configuration;
using Autumn.Functional;

namespace Autumn.Http.Sessions;

/// <summary>
/// Implements session management for HTTP requests, including retrieving, validating, and destroying sessions.
/// </summary>
public class AutumnHttpSessionManager : IHttpSessionManager {

    private readonly bool tokenIsQueryParameter;
    private readonly string tokenName;

    private readonly double sessionLifeTime;
    private readonly double sessionLifeExtension;

    private readonly Dictionary<string, IHttpSession> sessions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutumnHttpSessionManager"/> with specified static properties.
    /// </summary>
    /// <param name="staticPropertySource">The static property source containing configuration settings.</param>
    public AutumnHttpSessionManager([Inject] StaticPropertySource staticPropertySource) : this(staticPropertySource.GetValues("autumn.http.session.management")) {}

    /// <summary>
    /// Initializes a new instance of the <see cref="IDictionary{String, Object}"/> with specified static properties.
    /// </summary>
    /// <param name="parameters">The dictionary containing configuration settings.</param>
    public AutumnHttpSessionManager(IDictionary<string, object?> parameters) {
        this.sessions = [];
        if (parameters.GetOrElse("token-type", "cookie")?.ToLowerInvariant() is "query-param") {
            this.tokenIsQueryParameter = true;
        }
        this.tokenName = parameters.GetOrElse("token-name", "_s") ?? "_s";
        this.sessionLifeTime = parameters.GetOrElse("time", 30.0);
        this.sessionLifeExtension = parameters.GetOrElse("extension-time", 2.5);
    }

    /// <inheritdoc/>
    public IHttpSession? GetSession(HttpListenerContext context, IDictionary<string, string> queryParameters) {
        string domain = context.Request.Url!.Host;
        string tokenIdentifier = tokenIsQueryParameter switch {
            true => queryParameters.TryGetValue(tokenName, out string? token) ? token! : string.Empty,
            false => TryGetCookieToken(domain, context.Request.Cookies)
        };
        if (string.IsNullOrEmpty(tokenIdentifier)) {
            return this.RegisterSession(context);
        }
        IHttpSession? activeSession = sessions.TryGetValue(tokenIdentifier, out IHttpSession? session) ? session : null;
        if (activeSession is null || IsExpired(activeSession)) {
            return null;
        }
        IHttpSession updatedSession = activeSession.ExtendLifeSpan(TimeSpan.FromMinutes(sessionLifeExtension)); // Keep extending lifespan
        if (!tokenIsQueryParameter) {
            context.Response.SetCookie(updatedSession.ToCookie(tokenName));
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
        Cookie? cookie = cookies.FirstOrDefault(x => x.Name == tokenName && (x.Domain == domain || (x.Domain.Length == 0 && domain is "localhost")) && !x.Expired);
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
    private IHttpSession RegisterSession(HttpListenerContext context) {
        Guid guid = Guid.NewGuid();
        AutumnHttpSession session = new AutumnHttpSession(guid.ToString(), context.Request.Url!.Host, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(sessionLifeTime));
        if (!tokenIsQueryParameter) {
            context.Response.SetCookie(session.ToCookie(tokenName));
        }
        return sessions[guid.ToString()] = session;
    }

    /// <inheritdoc/>
    public void DestroyInactiveSessions() {
        foreach (var session in sessions.Values.Where(IsExpired)) {
            session.Destroy();
            sessions.Remove(session.SessionIdentifier);
        }
    }

}
