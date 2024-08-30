using System.Net;

using Autumn.Annotations;
using Autumn.Context;
using Autumn.Context.Configuration;

namespace Autumn.Http.Sessions;

/// <summary>
/// Extended HTTP session manager for managing multiple sessions accross different paths on the HTTP server.
/// </summary>
public sealed class AutumnHttpPathSessionManager : IHttpSessionManager { // TODO: Add support for parent/child sessions

    private record PathManager(string Path, string Manager);

    private readonly Dictionary<string, IHttpSessionManager> subManagers;
    private readonly Dictionary<IHttpSession, IHttpSessionManager> sessionOwners;

    /// <summary>
    /// Initialise a new <see cref="AutumnHttpPathSessionManager"/> instance.
    /// </summary>
    /// <param name="context">The application context to use when initialising submanagers</param>
    /// <param name="staticPropertySource">The static property configuration to use when initialising the session manager</param>
    public AutumnHttpPathSessionManager([Inject] AutumnAppContext context, [Inject] StaticPropertySource staticPropertySource) {
        subManagers = new();
        sessionOwners = new();
        PathManager[] paths = staticPropertySource.GetValueOrDefault("autumn.http.session.management.manager.paths", Array.Empty<PathManager>()) ?? Array.Empty<PathManager>();
        for (int i = 0; i < paths.Length; i++) {
            var path = paths[i];
            Type managerType = context.GetComponentType(path.Manager) ?? typeof(AutumnHttpSessionManager);
            IDictionary<string, object?> managerProperties = staticPropertySource.GetValues($"autumn.http.session.management.manager.paths.{(i+1)}");
            IHttpSessionManager managerInstance = context.CreateContextObject(managerType, new object[] { managerProperties }) as IHttpSessionManager ?? throw new Exception();
            subManagers.Add(path.Path, managerInstance);
        }
    }

    /// <inheritdoc/>
    public void DestroyInactiveSessions() {
        foreach (var manager in subManagers.Values) {
            manager.DestroyInactiveSessions();
        }
    }

    /// <inheritdoc/>
    public IHttpSession? GetSession(HttpListenerContext context, IDictionary<string, string> queryParameters) {
        IHttpSessionManager? manager = GetManager(context.Request.Url);
        if (manager is null) {
            return null;
        }
        IHttpSession? session = manager.GetSession(context, queryParameters);
        if (session is not null) {
            session.OnContextDestroyed += _ => sessionOwners.Remove(session);
            sessionOwners.Add(session, manager);
        }
        return session;
    }

    /// <inheritdoc/>
    public bool IsExpired(IHttpSession session) {
        if (sessionOwners.TryGetValue(session, out IHttpSessionManager? manager)) {
            return manager.IsExpired(session);
        }
        return true;
    }

    private IHttpSessionManager? GetManager(Uri? path) {
        if (path is null) {
            return null;
        }
        foreach (var (prefix, manager) in subManagers) {
            if (path.LocalPath.StartsWith(prefix)) {
                return manager;
            }
        }
        return null;
    }

}
