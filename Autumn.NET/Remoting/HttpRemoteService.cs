using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;

using Autumn.Annotations;
using Autumn.Context;
using Autumn.Context.Configuration;
using Autumn.Http;
using Autumn.Http.Annotations;
using Autumn.Proxying;
using Autumn.Types;

namespace Autumn.Remoting;

/// <summary>
/// Class representing a remote service handler for <typeparamref name="T"/> with calls made using HTTP(S).
/// </summary>
/// <typeparam name="T">The type of the remote service wrap. If <typeparamref name="T"/> is a <c>class</c>, only methods marked <c>virtual</c> will be callable.</typeparam>
public sealed class HttpRemoteService<T> : IProxy {

    private readonly record struct EndpointResult(bool Failed, HttpStatusCode StatusCode, object? Content);

    private sealed class Endpoint(string uri, string method, string[] queryNames, int bodyArgument, Type returnType) {
        public EndpointResult Invoke(HttpRemoteService<T> service, object?[] arguments) {

            StringBuilder stringBuilder = new StringBuilder(uri);
            if (queryNames.Length > 0) {
                stringBuilder.Append('?');
            }
            for (int i = 0; i < queryNames.Length; i++) {
                if (i == bodyArgument)
                    continue;
                if (i > 0 && bodyArgument != i - 1)
                    stringBuilder.Append('&');
                stringBuilder.Append(queryNames[i]);
                stringBuilder.Append('=');
                stringBuilder.Append(arguments[i]); // TODO: Check it's a primitive
            }

            HttpContent? httpContent = null;
            if (bodyArgument != -1) {
                httpContent = JsonContent.Create(arguments[bodyArgument]);
            }

            HttpRequestMessage request = new() {
                Method = method switch {
                    "GET" => HttpMethod.Get,
                    "POST" => HttpMethod.Post,
                    "PUT" => HttpMethod.Put,
                    "DELETE" => HttpMethod.Delete,
                    _ => throw new NotSupportedException($"Http method '{method}' not supported")
                },
                RequestUri = new Uri(service.BaseUri, stringBuilder.ToString()),
                Content = httpContent
                // TODO: Configure headers
            };
            
            var response = service._httpClient.Send(request);
            if (!response.IsSuccessStatusCode)
                return new EndpointResult(true, response.StatusCode, null);

            using var bodyStream = response.Content.ReadAsStream();
            if (returnType.IsPrimitive)
                return new EndpointResult(false, response.StatusCode, TypeConverter.Convert(new StreamReader(bodyStream).ReadToEnd(), typeof(string), returnType));

            return new EndpointResult(false, response.StatusCode, JsonSerializer.Deserialize(bodyStream, returnType));

        }
    }

    private const string PROPERTY_BASE_URI = "autumn.remote.http.base-uri";

    private readonly IHttpClient _httpClient;
    private readonly Dictionary<string, Endpoint> _endpoints = [];

    private readonly T? _proxyInstance;

    /// <summary>
    /// Gets the underlying <typeparamref name="T"/> instance that performs the remote calls.
    /// </summary>
    public T Service {
        get => _proxyInstance ?? throw new Exception("Uninitialized");
    }

    /// <summary>
    /// Get or set the base <see cref="Uri"/> to create request urls from.
    /// </summary>
    public Uri BaseUri {  get; set; }

    /// <summary>
    /// Initialize a new <see cref="HttpRemoteService{T}"/> instance with a <see cref="IHttpClient"/> for making remote HTTP(s) requests and a <see cref="StaticPropertySource"/> for further initialization.
    /// </summary>
    /// <param name="httpClient">The HTTP client to make requests through</param>
    /// <param name="properties">Property source for initializing the remote service</param>
    /// <exception cref="InvalidPropertyException"/>
    public HttpRemoteService([Inject] IHttpClient httpClient, [Inject] StaticPropertySource properties) {
        
        // Set fields
        _httpClient = httpClient;
        _proxyInstance = Proxy.CreateProxy<T>(this);

        var baseUri = properties.GetValueOrDefault(PROPERTY_BASE_URI, "https://localhost:8443") ?? throw new InvalidPropertyException(PROPERTY_BASE_URI, "null", "Property value cannot be null");
        if (!Uri.TryCreate(baseUri, UriKind.Absolute, out var uri)) {
            throw new InvalidPropertyException(PROPERTY_BASE_URI, baseUri);
        }
        BaseUri = uri;
        
        InitEndpoints();

    }

    private void InitEndpoints() {

        // Grab endpoint methods
        var methods = typeof(T)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(x => (x, x.GetCustomAttribute<EndpointAttribute>()))
            .Where(x => x.Item2 is not null)
            .ToArray();

        // Register in endpoint list
        foreach (var (method, endpoint) in methods) {
            var methodParams = method.GetParameters();
            var bodyArg = Array.FindIndex(methodParams, x => x.GetCustomAttribute<BodyAttribute>() is not null);
            var queryParams = new string[methodParams.Length];
            for (int i = 0; i < methodParams.Length; i++) {
                queryParams[i] = methodParams[i].Name ?? throw new InvalidOperationException("Cannot invoke method where at least one parameter is nameless");
            }
            _endpoints[MethodToString(method)] = new Endpoint(endpoint!.Endpoint, endpoint.Method, queryParams, bodyArg, method.ReturnType);
        }

    }

    /// <inheritdoc/>
    public object? HandleMethod(MethodInfo targetMethod, object?[] arguments) {
        if (!_endpoints.TryGetValue(MethodToString(targetMethod), out var endpoint)) {
            throw new NotSupportedException($"Target method {targetMethod.Name} is not a valid service endpoint and cannot be invoked remotely.");
        }
        EndpointResult result = endpoint.Invoke(this, arguments);
        if (result.Failed)
            throw new RemoteServiceException($"Remote service yielded unexpected status code {result.StatusCode}");

        return result.Content;
    }

    private static string MethodToString(MethodInfo method) => $"{method.Name}({string.Join(',',method.GetParameters().Select(x => x.ParameterType))}):{method.ReturnType}";

}
