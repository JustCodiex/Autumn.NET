using System.Net;
using System.Reflection;
using System.Text.Json;

using Autumn.Context;
using Autumn.Context.Configuration;
using Autumn.Http;
using Autumn.Http.Annotations;
using Autumn.Http.Interceptors;
using Autumn.Http.Sessions;
using Autumn.Scheduling;
using Autumn.Test.TestHelpers;

namespace Autumn.Test.Http;

public sealed class AutumnHttpServerTest : IDisposable {

    private static readonly Func<Dictionary<string, object?>> DefaultConfig = () => new Dictionary<string, object?>() {
        { "autumn.http.port", "8080" }
    };

    private AutumnHttpServer? server;

    public void Dispose() {
        if (server is not null) {
            server.Shutdown();
            TimedAssert.True(() => !server.IsListening);
        }
    }

    private static readonly Func<HttpClient> DefaultHttpClient = () => new();

    private static readonly Func<(HttpClient, CookieContainer)> DefaultCookieHttpClient = () => {
        CookieContainer cookies = new CookieContainer();
        HttpClientHandler handler = new HttpClientHandler {
            CookieContainer = cookies,
            UseCookies = true
        };
        return (new HttpClient(handler), cookies);
    };

    [Fact]
    public void CanCreateHttpServer() {
        
        // Init context
        AutumnAppContext appContext = new AutumnAppContext();
        appContext.RegisterComponent(new StaticPropertySource(DefaultConfig()));
        
        // Create server and start it
        server = new AutumnHttpServer(appContext);
        Task serverTask = Task.Run(server.Start);

        TimedAssert.True(() => server.IsListening);

    }

    private class SimpleEndpoint {

        public static readonly MethodInfo CallMeMethod = typeof(SimpleEndpoint).GetMethod(nameof(CallMe), BindingFlags.Public | BindingFlags.Instance) ?? throw new Exception("Failed getting call me method");
        public static readonly EndpointAttribute CallMeEndpointAttribute = CallMeMethod.GetCustomAttribute<EndpointAttribute>() ?? throw new Exception("Failed getting endpoint attribute");

        [Endpoint("/callme")]
        public HttpResponse<string> CallMe() => HttpResponse.Ok("Hello World!");

        public static readonly MethodInfo IThrowErrorsMethod = typeof(SimpleEndpoint).GetMethod(nameof(IThrowErrors), BindingFlags.Public | BindingFlags.Instance) ?? throw new Exception("Failed getting error method");
        public static readonly EndpointAttribute IThrowErrorsMethodEndpointAttribute = IThrowErrorsMethod.GetCustomAttribute<EndpointAttribute>() ?? throw new Exception("Failed getting endpoint attribute");

        [Endpoint("/throwserrors")]
        public HttpResponse IThrowErrors() => throw new Exception("I was an exception");

        [EndpointExceptionHandler(typeof(Exception))]
        public HttpResponse GenericExceptionHandler(Exception exception) => HttpResponse.InternalServerError(exception.Message);

    }

    [Fact]
    public void CanRespondToHttpRequest() {

        // Init context
        AutumnAppContext appContext = new AutumnAppContext();
        appContext.RegisterComponent(new StaticPropertySource(DefaultConfig()));

        // Create endpoint
        SimpleEndpoint endpoint = new SimpleEndpoint();

        // Create server and start it
        server = new AutumnHttpServer(appContext);
        server.RegisterEndpoint(endpoint, SimpleEndpoint.CallMeMethod, SimpleEndpoint.CallMeEndpointAttribute);
        Task serverTask = Task.Run(server.Start);

        // Assert it's running
        TimedAssert.True(() => server.IsListening);

        // Get http client and call our rendpoint
        HttpClient client = DefaultHttpClient();
        HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Get, "http://localhost:8080/callme"));
        using StreamReader responseReader = new StreamReader(response.Content.ReadAsStream());

        // Assert on endpoint response
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Hello World!", responseReader.ReadToEnd());

    }

    [Fact]
    public void CanHandleErrors() {

        // Init context
        AutumnAppContext appContext = new AutumnAppContext();
        appContext.RegisterComponent(new StaticPropertySource(DefaultConfig()));

        // Create endpoint
        SimpleEndpoint endpoint = new SimpleEndpoint();
        appContext.RegisterComponent(endpoint);

        // Create server and start it
        server = new AutumnHttpServer(appContext);
        server.RegisterEndpoint(endpoint, SimpleEndpoint.IThrowErrorsMethod, SimpleEndpoint.IThrowErrorsMethodEndpointAttribute);
        Task serverTask = Task.Run(server.Start);

        // Assert it's running
        TimedAssert.True(() => server.IsListening);

        // Get http client and call our rendpoint
        HttpClient client = DefaultHttpClient();
        HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Get, "http://localhost:8080/throwserrors"));
        using StreamReader responseReader = new StreamReader(response.Content.ReadAsStream());

        // Assert on endpoint response
        Assert.Equal("I was an exception", responseReader.ReadToEnd());

    }

    private class SimpleSessionEndpoint {

        public static readonly MethodInfo CounterMethod = typeof(SimpleSessionEndpoint).GetMethod(nameof(Counter), BindingFlags.Public | BindingFlags.Instance) ?? throw new Exception("Failed getting counter method");
        public static readonly EndpointAttribute CounterMethodEndpointAttribute = CounterMethod.GetCustomAttribute<EndpointAttribute>() ?? throw new Exception("Failed getting endpoint attribute");

        [Endpoint("/counter", Method = "POST")]
        public int Counter(IHttpSession session) {
            int counter = session.GetSessionValue<int>("count");
            counter++;
            session.StoreSessionValue("count", counter);
            return counter;
        }

    }

    [Fact]
    public void WillStoreSession() {

        // Get config
        var cfg = DefaultConfig();
        cfg["autumn.http.session.management.enabled"] = true;

        // Init context
        AutumnAppContext appContext = new AutumnAppContext();
        appContext.RegisterComponent(new AutumnScheduler());
        appContext.RegisterComponent(new StaticPropertySource(cfg));

        // Create endpoint
        SimpleSessionEndpoint endpoint = new SimpleSessionEndpoint();

        // Create server and start it
        server = new AutumnHttpServer(appContext);
        server.RegisterEndpoint(endpoint, SimpleSessionEndpoint.CounterMethod, SimpleSessionEndpoint.CounterMethodEndpointAttribute);
        Task serverTask = Task.Run(server.Start);

        // Assert it's running
        TimedAssert.True(() => server.IsListening);

        // Get http client and call our rendpoint
        var (client, cookies) = DefaultCookieHttpClient();
        Assert.True(cookies.Count == 0);
        HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Post, "http://localhost:8080/counter"));
        StreamReader responseReader = new StreamReader(response.Content.ReadAsStream());

        // Assert on endpoint response
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("1", responseReader.ReadToEnd());
        Assert.Contains(cookies.GetAllCookies(), x => x.Name == "_s");

        // Get http client and call our rendpoint
        response = client.Send(new HttpRequestMessage(HttpMethod.Post, "http://localhost:8080/counter"));
        responseReader = new StreamReader(response.Content.ReadAsStream());

        // Assert on endpoint response
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("2", responseReader.ReadToEnd());

    }

    private class ComplexSessionEndpoint {

        public static readonly MethodInfo CounterAMethod = typeof(ComplexSessionEndpoint).GetMethod(nameof(CounterA), BindingFlags.Public | BindingFlags.Instance) ?? throw new Exception("Failed getting counter method");
        public static readonly EndpointAttribute CounterAMethodEndpointAttribute = CounterAMethod.GetCustomAttribute<EndpointAttribute>() ?? throw new Exception("Failed getting endpoint attribute");

        [Endpoint("/serviceA/counter", Method = "PUT")]
        public int CounterA(IHttpSession session) => count(session);

        public static readonly MethodInfo CounterBMethod = typeof(ComplexSessionEndpoint).GetMethod(nameof(CounterB), BindingFlags.Public | BindingFlags.Instance) ?? throw new Exception("Failed getting counter method");
        public static readonly EndpointAttribute CounterBMethodEndpointAttribute = CounterBMethod.GetCustomAttribute<EndpointAttribute>() ?? throw new Exception("Failed getting endpoint attribute");

        [Endpoint("/serviceB/counter", Method = "PUT")]
        public int CounterB(IHttpSession session) => count(session);

        private static int count(IHttpSession session) {
            int counter = session.GetSessionValue<int>("count");
            counter++;
            session.StoreSessionValue("count", counter);
            return counter;
        }

    }

    [Fact]
    public void CanHandleMultipleSessionsPerPath() {

        string cfgStr = """
            autumn:
                http:
                    port: 8080
                    session:
                        management:
                            enabled: true
                            type: Autumn.Http.Sessions.AutumnHttpPathSessionManager
                            manager:
                                paths:
                                    - path: /serviceA/
                                      manager: Autumn.Http.Sessions.AutumnHttpSessionManager
                                    - path: /serviceB/
                                      manager: Autumn.Http.Sessions.AutumnHttpSessionManager
                                      token-name: _sesh
            """;
        var cfg = ConfigHelper.ConfigOf(cfgStr);

        // Init context
        AutumnAppContext appContext = new AutumnAppContext();
        appContext.RegisterComponent(new AutumnScheduler());
        appContext.RegisterComponent(cfg);

        // Create endpoint
        ComplexSessionEndpoint endpoint = new ComplexSessionEndpoint();

        // Create server and start it
        server = new AutumnHttpServer(appContext);
        server.RegisterEndpoint(endpoint, ComplexSessionEndpoint.CounterAMethod, ComplexSessionEndpoint.CounterAMethodEndpointAttribute);
        server.RegisterEndpoint(endpoint, ComplexSessionEndpoint.CounterBMethod, ComplexSessionEndpoint.CounterBMethodEndpointAttribute);
        Task serverTask = Task.Run(server.Start);

        // Assert it's running
        TimedAssert.True(() => server.IsListening);

        // Get http client and call our rendpoint
        var (client, cookies) = DefaultCookieHttpClient();
        Assert.True(cookies.Count == 0);
        HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Put, "http://localhost:8080/serviceA/counter"));
        StreamReader responseReader = new StreamReader(response.Content.ReadAsStream());

        // Assert on endpoint response
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("1", responseReader.ReadToEnd());
        Assert.Contains(cookies.GetAllCookies(), x => x.Name == "_s");

        // Get http client and call our rendpoint
        response = client.Send(new HttpRequestMessage(HttpMethod.Put, "http://localhost:8080/serviceB/counter"));
        responseReader = new StreamReader(response.Content.ReadAsStream());

        // Assert on endpoint response
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("1", responseReader.ReadToEnd());
        Assert.Contains(cookies.GetAllCookies(), x => x.Name == "_sesh");

    }

    private class RequestResponse : IHttpRequestInterceptor, IHttpResponseInterceptor {

        public List<string> Requests { get; set; } = [];

        public List<string> Responses { get; set; } = [];

        public bool HandleError(HttpListenerResponse response, IHttpSession? session) => true;

        public bool Intercept(IHttpRequest request, IHttpSession? session) {
            Requests.Add(request.GetBodyAsString());
            request.ResetBodyStreamPosition();
            return true;
        }

        public bool Intercept(object interceptedResult, HttpListenerResponse response, IHttpSession? session, out object result) {
            Responses.Add(interceptedResult.ToString()!);
            result = interceptedResult;
            return true;
        }

    }

    private class CalculatorEndpoint {

        public static readonly MethodInfo AdderMethod = typeof(CalculatorEndpoint).GetMethod(nameof(Adder), BindingFlags.Public | BindingFlags.Instance) ?? throw new Exception("Failed getting adder method");
        public static readonly EndpointAttribute AdderEndpointAttribute = AdderMethod.GetCustomAttribute<EndpointAttribute>() ?? throw new Exception("Failed getting endpoint attribute");

        public class Payload {
            public int X { get; set; }
            public int Y { get; set; }
        }

        [Endpoint("/add", Method = "POST")]
        public int Adder([Body] Payload body) => body.X + body.Y;

    }

    [Fact]
    public void CanIntercept() {

        // Get config
        string cfgStr = $"""
            autumn:
                http:
                    port: 8080
                    flow:
                        interceptors:
                        -  {typeof(RequestResponse).FullName!}
            """;
        var cfg = ConfigHelper.ConfigOf(cfgStr);

        // Create RR
        var rr = new RequestResponse();

        // Init context
        AutumnAppContext appContext = new AutumnAppContext();
        appContext.RegisterComponent(new AutumnScheduler());
        appContext.RegisterComponent(rr);
        appContext.RegisterComponent(cfg);

        // Create endpoint
        CalculatorEndpoint endpoint = new CalculatorEndpoint();

        // Create server and start it
        server = new AutumnHttpServer(appContext);
        server.RegisterEndpoint(endpoint, CalculatorEndpoint.AdderMethod, CalculatorEndpoint.AdderEndpointAttribute);
        Task serverTask = Task.Run(server.Start);

        // Assert it's running
        TimedAssert.True(() => server.IsListening);

        // Get http client and call our rendpoint
        var (client, cookies) = DefaultCookieHttpClient();
        Assert.True(cookies.Count == 0);
        HttpResponseMessage response = client.Send(new HttpRequestMessage(HttpMethod.Post, "http://localhost:8080/add") { 
            Content = new StringContent(JsonSerializer.Serialize(new CalculatorEndpoint.Payload { 
                X = 5, 
                Y = 10 
            })) 
        });
        Assert.True(response.IsSuccessStatusCode);
        StreamReader responseReader = new StreamReader(response.Content.ReadAsStream());
        
        string result = responseReader.ReadToEnd();
        Assert.Equal("15", result);

        Assert.Single(rr.Requests);
        Assert.Equal("{\"X\":5,\"Y\":10}", rr.Requests.First());

        Assert.Single(rr.Responses);
        Assert.Equal("15", rr.Responses.First());

    }

}
