using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

using Autumn.Context;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="AutumnHttpServer"/> class with the specified application context.
    /// </summary>
    /// <param name="context">The application context.</param>
    internal AutumnHttpServer(AutumnAppContext context) {
        _appContext = context;
        _endpoints = new();
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:8080/");
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

        _listener.Start();
        while(_listener.IsListening) {

            // Accept
            var next = _listener.GetContext();

            // Handle
            Task.Run(() => HandleIncoming(next));

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
            MapObjectToHttpResponse(result, listenerContext.Response);
        } else {
            listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
            listenerContext.Response.Close();
        }

    }

    private Dictionary<string, string> ParseQuery(Uri uri) {
        string query = uri.Query.TrimStart('?');
        Dictionary<string, string> result = new();
        string[] parameters = query.Split('&');
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
            if (parameters.TryGetValue(methodArgs[i].Name!, out string? paramValue)) {
                callArgs[i] = MapStringToType(paramValue, methodArgs[i].ParameterType);
            }
        }
        return callArgs;
    }

    private object? MapStringToType(string value, Type targetType) {
        if (targetType == typeof(string)) {
            return value;
        } else if (targetType == typeof(float) && float.TryParse(value, out float f)) {
            return f;
        }
        return null;
    }

    private void MapObjectToHttpResponse(object value, HttpListenerResponse response) {
        switch (value) {
            case string s:
                response.Headers.Add("Content-Type", "text/html, charset=\"UTF-8\"");
                response.OutputStream.Write(Encoding.UTF8.GetBytes(s));
                break;
            default:
                response.Headers.Add("Content-Type", "text/json, charset=\"UTF-8\"");
                byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
                response.OutputStream.Write(data);
                break;
        }
        response.OutputStream.Close();
    }

}
