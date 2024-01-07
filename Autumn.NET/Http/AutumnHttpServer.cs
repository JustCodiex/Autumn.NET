using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Autumn.Annotations;
using Autumn.Context;
using Autumn.Context.Configuration;
using Autumn.Http.Annotations;
using Autumn.Http.Sessions;
using Autumn.Scheduling;

namespace Autumn.Http;

/// <summary>
/// Represents an HTTP server for handling incoming HTTP requests and routing them to registered endpoints.
/// </summary>
public sealed class AutumnHttpServer {

    private record HttpEndpointMethod(object Target, MethodInfo Info);
    private record HttpEndpoint(string Path, 
        HttpEndpointMethod? GetMethod, 
        HttpEndpointMethod? PostMethod, 
        HttpEndpointMethod? PutMethod, 
        HttpEndpointMethod? DeleteMethod);

    private readonly AutumnAppContext _appContext;
    private readonly Dictionary<string, HttpEndpoint> _endpoints;

    private readonly HttpListener _listener;

    private JsonSerializerOptions _serializerOptions = new();
    private string _corsHeader = string.Empty;

    private IHttpSessionManager? _sessionManager;

    private bool _isListenerInitialised;

    /// <summary>
    /// Gets a value indicating if the AutumnHTTP server can be used on the current operating system.
    /// </summary>
    public static bool IsSupported => HttpListener.IsSupported;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutumnHttpServer"/> class with the specified application context.
    /// </summary>
    /// <param name="context">The application context.</param>
    internal AutumnHttpServer(AutumnAppContext context) {
        _appContext = context;
        _endpoints = new();
        _listener = new HttpListener();
    }

    private void InitListener() {
        if (_isListenerInitialised)
            return;

        StaticPropertySource staticPropertySource = _appContext.GetInstanceOf<StaticPropertySource>();
        int port = staticPropertySource.GetValueOrDefault("autumn.http.port", 80);
        string? host = staticPropertySource.GetValueOrDefault("autumn.http.host", "localhost");
        bool isHttp = staticPropertySource.GetValueOrDefault("autumn.http.disableHttps", true); // Should probably invert this at some point

        StringBuilder sb = new StringBuilder();
        if (isHttp) {
            sb.Append("http://");
        } else {
            sb.Append("https://");
        }

        _serializerOptions = new() {
            IncludeFields = staticPropertySource.GetValueOrDefault("autumn.http.serialiser.json.includeFields", false),
            DefaultIgnoreCondition = staticPropertySource.GetValueOrDefault("autumn.http.serialiser.json.ignoreCondition", JsonIgnoreCondition.WhenWritingDefault)
        };

        _listener.Prefixes.Add(sb.Append(host).Append(':').Append(port).Append('/').ToString());

        _corsHeader = staticPropertySource.GetValueOrDefault("autumn.http.headers.cors", string.Empty) ?? string.Empty;

        if (staticPropertySource.GetValueOrDefault("autumn.http.session.management.enabled", false)) {
            InitSessionManager(staticPropertySource);
        }

        _isListenerInitialised = true;

    }

    private void InitSessionManager(StaticPropertySource staticPropertySource) {
        string? sessionKlass = staticPropertySource.GetValueOrDefault("autumn.http.session.management.type", typeof(AutumnHttpSessionManager).FullName);
        if (string.IsNullOrEmpty(sessionKlass)) {
            return; // TODO: Log
        }
        Type? type = _appContext.GetComponentType(sessionKlass);
        if (type is null) {
            return; // TODO: Log
        }
        _sessionManager = _appContext.GetInstanceOf(type, staticPropertySource) as IHttpSessionManager;
        if (_sessionManager is not null) {
            AutumnScheduler scheduler = _appContext.GetInstanceOf<AutumnScheduler>();
            scheduler.Schedule(_sessionManager, type.GetMethod(nameof(IHttpSessionManager.DestroyInactiveSessions), BindingFlags.Public | BindingFlags.Instance) ?? throw new Exception("Unable to schedule even listener destruction"), Schedule.EveryNthSecond(30));
        }
    }

    /// <summary>
    /// Registers an endpoint for handling HTTP requests.
    /// </summary>
    /// <param name="target">The target object that contains the endpoint method.</param>
    /// <param name="method">The endpoint method.</param>
    /// <param name="endpointAttribute">The attribute specifying the endpoint details.</param>
    public void RegisterEndpoint(object target, MethodInfo method, EndpointAttribute endpointAttribute) {
        string endpoint = endpointAttribute.Endpoint;
        var endpointMethod = new HttpEndpointMethod(target, method);
        if (_endpoints.TryGetValue(endpoint, out var ep)) {
            _endpoints[endpoint] = OverrideMethod(ep, endpointAttribute.Method, endpointMethod);
            return;
        }
        var newHandler = OverrideMethod(new HttpEndpoint(endpoint, null, null, null, null), endpointAttribute.Method, endpointMethod);
        _endpoints.Add(endpoint, newHandler);
    }

    private HttpEndpoint OverrideMethod(HttpEndpoint endpoint, string method, HttpEndpointMethod handler) {
        return method switch {
            "GET" => endpoint with { GetMethod = handler },
            "POST" => endpoint with { PostMethod = handler },
            "PUT" => endpoint with { PutMethod = handler },
            "DELETE" => endpoint with { DeleteMethod = handler },
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Starts the HTTP server and begins listening for incoming requests.
    /// </summary>
    public void Start() {

        // Init listener
        InitListener();

        _listener.Start();
        while(_listener.IsListening) {
            try {
                // Accept
                var next = _listener.GetContext();

                // Handle
                Task.Run(() => HandleIncoming(next));

            } catch (HttpListenerException) {
                // TODO: Allow user to configure what to do next
            } catch {
                throw;
            }
        }

    }

    private void HandleIncoming(HttpListenerContext listenerContext) {

        // Get request url
        Uri? requestUrl = listenerContext.Request.Url;
        if (requestUrl is null) {
            listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            listenerContext.Response.Close();
            return;
        }

        // Get the local path
        string localPath = requestUrl.LocalPath;
        if (!_endpoints.TryGetValue(localPath, out HttpEndpoint? endpoint)) {
            listenerContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            listenerContext.Response.Close();
            return;
        }

        // Get handler
        bool isHeadMethod = listenerContext.Request.HttpMethod.Equals("HEAD", StringComparison.InvariantCultureIgnoreCase);
        var handler = listenerContext.Request.HttpMethod.ToUpper() switch {
            "GET" => endpoint.GetMethod,
            "HEAD" => endpoint.GetMethod,
            "POST" => endpoint.PostMethod,
            "PUT" => endpoint.PutMethod,
            "DELETE" => endpoint.DeleteMethod,
            _ => null
        };
        if (handler is null) {
            listenerContext.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            listenerContext.Response.Close();
            return;
        }

        // Add CORS headers
        listenerContext.Response.Headers.Add("Access-Control-Allow-Origin", this._corsHeader);

        // Parse query
        var query = ParseQuery(requestUrl);

        // Get session (if any)
        if (!GetSession(listenerContext, query, out IHttpSession? session)) {
            // Run session expired handler
        }

        // Map to arguments
        object?[] callArgs = MapQueryParameters(query, handler.Info, session);
        if (!GetRequestBody(listenerContext, callArgs, handler.Info)) {
            listenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            listenerContext.Response.Close();
            return;
        }

        // Invoke
        object? result = null;
        try {
            result = handler.Info.Invoke(handler.Target, callArgs);
        } catch (Exception ex) {
            listenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            listenerContext.Response.StatusDescription = ex.Message;
            listenerContext.Response.Close();
        }

        // Return
        if (!isHeadMethod && result is not null && handler.Info.ReturnType != typeof(void)) {
            ContentTypeAttribute? contentTypeOverride = handler.Info.ReturnParameter.GetCustomAttribute<ContentTypeAttribute>();
            MapObjectToHttpResponse(result, listenerContext.Response, contentTypeOverride);
        } else {
            listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
            listenerContext.Response.Close();
        }

    }

    private Dictionary<string, string> ParseQuery(Uri uri) {
        string query = uri.Query.TrimStart('?');
        Dictionary<string, string> result = new();
        string[] parameters = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parameters.Length; i++) {
            int eq = parameters[i].IndexOf('=');
            result[parameters[i][..eq].Trim()] = parameters[i][(eq+1)..].Trim();
        }
        return result;
    }

    private object?[] MapQueryParameters(Dictionary<string, string> parameters, MethodInfo methodInfo, IHttpSession? session) {
        var methodArgs = methodInfo.GetParameters();
        object?[] callArgs = new object?[methodArgs.Length];
        for (int i = 0; i < callArgs.Length; i++) {
            if (methodArgs[i].GetCustomAttribute<BodyAttribute>() is not null) {
                continue;
            } else if (methodArgs[i].GetCustomAttribute<InjectAttribute>() is InjectAttribute injectAttribute) {
                callArgs[i] = _appContext.SolveInjectDependency(methodArgs[i].ParameterType, methodArgs[i].Name ?? string.Empty, injectAttribute, session);
                continue;
            } else if (methodArgs[i].ParameterType.IsSubclassOf(typeof(IHttpSession)) || methodArgs[i].ParameterType == typeof(IHttpSession)) { // The session object is specifically requested
                callArgs[i] = session;
                continue;
            }
            var paramName = methodArgs[i].GetCustomAttribute<ParameterAttribute>() is ParameterAttribute p ? p.Name : methodArgs[i].Name!;
            if (parameters.TryGetValue(paramName, out string? paramValue)) {
                callArgs[i] = MapStringToType(SanitiseQueryString(paramValue), methodArgs[i].ParameterType);
            }
        }
        return callArgs;
    }

    private bool GetRequestBody(HttpListenerContext context, object?[] currentArgs, MethodInfo methodInfo) {
        ParameterInfo[] parameterInfos = methodInfo.GetParameters();
        ParameterInfo? bodyParameter = parameterInfos.FirstOrDefault(x => x.GetCustomAttribute<BodyAttribute>() is not null);
        if (bodyParameter is null) {
            return true;
        }

        int bodyParameterIndex = Array.IndexOf(parameterInfos, bodyParameter);
        if (!context.Request.HasEntityBody) {
            return false;
        }

        if (currentArgs[bodyParameterIndex] is not null) {
            return false;
        }

        // Else, try through json
        switch (context.Request.ContentType?.ToLower()) {
            case "text/plain" or "text/html" when bodyParameter.ParameterType == typeof(string): {
                using var streamReader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                currentArgs[bodyParameterIndex] = streamReader.ReadToEnd();
                return true;
            }
            case "application/xml":
            case "text/xml":
                throw new NotImplementedException();
            case "application/x-www-form-urlencoded":
                throw new NotImplementedException();
            case "application/json":
            default:
                try {
                    currentArgs[bodyParameterIndex] = JsonSerializer.Deserialize(context.Request.InputStream, bodyParameter.ParameterType, options: _serializerOptions);
                } catch {
                    return false;
                }
                return true;
        }
        
    }

    private object? MapStringToType(string value, Type targetType) {
        if (targetType == typeof(string)) {
            return value;
        } else if (targetType == typeof(float) && float.TryParse(value, out float f)) {
            return f;
        } else if (targetType == typeof(int) && int.TryParse(value, out int i32)) {
            return i32;
        }
        return null;
    }

    private string SanitiseQueryString(string value) // https://documentation.n-able.com/N-central/userguide/Content/Further_Reading/API_Level_Integration/API_Integration_URLEncoding.html (TODO: Add remaining)
        => value.Replace("%20", " ").Replace("%2B", "+").Replace("%24", "$").Replace("%26", "&");

    private bool GetSession(HttpListenerContext context, Dictionary<string, string> queryParams, out IHttpSession? session) {
        session = this._sessionManager?.GetSession(context, queryParams);
        if (session is null) {
            return false;
        }
        // Session can only be non-null if session manager is defined
        if (this._sessionManager!.IsExpired(session)) {
            return false;
        }
        return true;
    }

    private void MapObjectToHttpResponse(object value, HttpListenerResponse response, ContentTypeAttribute? contentTypeOverwrite) {
        switch (value) {
            case string s:
                SetContentTypeOrDefault(response, contentTypeOverwrite, "text/html; charset='utf-8'");
                response.OutputStream.Write(Encoding.UTF8.GetBytes(s));
                break;
            default:
                SetContentTypeOrDefault(response, contentTypeOverwrite, "text/json; charset='utf-8'");
                byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, options: _serializerOptions));
                response.OutputStream.Write(data);
                break;
        }
        response.OutputStream.Close();
    }

    private void SetContentTypeOrDefault(HttpListenerResponse response, ContentTypeAttribute? contentType, string defaultType) {
        string contentTypeHeader = contentType is null ? defaultType : contentType.ContentType;
        foreach (var contentTypeEntry in contentTypeHeader.Split(';'))
            response.Headers.Add("Content-Type", contentTypeEntry);
    }

    /// <summary>
    /// Shuts down the integrated Autumn HTTP server
    /// </summary>
    public void Shutdown() {
        try {
            _listener.Stop();
        } catch { }
    }

}
