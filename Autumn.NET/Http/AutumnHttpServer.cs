using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

using Autumn.Annotations;
using Autumn.Context;
using Autumn.Context.Configuration;
using Autumn.Http.Annotations;

namespace Autumn.Http;

/// <summary>
/// Represents an HTTP server for handling incoming HTTP requests and routing them to registered endpoints.
/// </summary>
public sealed class AutumnHttpServer {

    private record HttpEndpointMethod(object Target, MethodInfo Info);
    private record HttpEndpoint(string Path, HttpEndpointMethod? GetMethod, HttpEndpointMethod? PostMethod);

    private readonly AutumnAppContext _appContext;
    private readonly Dictionary<string, HttpEndpoint> _endpoints;

    private readonly HttpListener _listener;

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

        _listener.Prefixes.Add(sb.Append(host).Append(':').Append(port).Append('/').ToString());

        _isListenerInitialised = true;

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
        var newHandler = OverrideMethod(new HttpEndpoint(endpoint, null, null), endpointAttribute.Method, endpointMethod);
        _endpoints.Add(endpoint, newHandler);
    }

    private HttpEndpoint OverrideMethod(HttpEndpoint endpoint, string method, HttpEndpointMethod handler) {
        return method switch {
            "GET" => endpoint with { GetMethod = handler },
            "POST" => endpoint with { PostMethod = handler },
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
        var handler = listenerContext.Request.HttpMethod switch {
            "GET" => endpoint.GetMethod,
            "POST" => endpoint.PostMethod,
            _ => null
        };
        if (handler is null) {
            listenerContext.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            listenerContext.Response.Close();
            return;
        }

        // Parse query
        var query = ParseQuery(requestUrl);

        // Map to arguments
        object?[] callArgs = MapQueryParameters(query, handler.Info);
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
        if (result is not null && handler.Info.ReturnType != typeof(void)) {
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

    private object?[] MapQueryParameters(Dictionary<string, string> parameters, MethodInfo methodInfo) {
        var methodArgs = methodInfo.GetParameters();
        object?[] callArgs = new object?[methodArgs.Length];
        for (int i = 0; i < callArgs.Length; i++) {
            if (methodArgs[i].GetCustomAttribute<BodyAttribute>() is not null) {
                continue;
            }
            if (methodArgs[i].GetCustomAttribute<InjectAttribute>() is InjectAttribute injectAttribute) {
                callArgs[i] = _appContext.SolveInjectDependency(methodArgs[i].ParameterType, methodArgs[i].Name ?? string.Empty, injectAttribute);
                continue;
            }
            var paramName = methodArgs[i].GetCustomAttribute<ParameterAttribute>() is ParameterAttribute p ? p.Name : methodArgs[i].Name!;
            if (parameters.TryGetValue(paramName, out string? paramValue)) {
                callArgs[i] = MapStringToType(paramValue, methodArgs[i].ParameterType);
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
                    currentArgs[bodyParameterIndex] = JsonSerializer.Deserialize(context.Request.InputStream, bodyParameter.ParameterType);
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
        }
        return null;
    }

    private void MapObjectToHttpResponse(object value, HttpListenerResponse response, ContentTypeAttribute? contentTypeOverwrite) {
        switch (value) {
            case string s:
                SetContentTypeOrDefault(response, contentTypeOverwrite, "text/html;charset='utf-8'");
                response.OutputStream.Write(Encoding.UTF8.GetBytes(s));
                break;
            default:
                SetContentTypeOrDefault(response, contentTypeOverwrite, "text/json;charset='UTF-8'");
                byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
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
