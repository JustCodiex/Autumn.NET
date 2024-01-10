using System.Net;
using System.Reflection;

using Autumn.Context;
using Autumn.Context.Configuration;
using Autumn.Http;
using Autumn.Http.Annotations;
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

    private static readonly Func<HttpClient> DefaultHttpClient = () => {
        return new HttpClient();
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

        public static readonly MethodInfo CallMeMethod = typeof(SimpleEndpoint).GetMethod("CallMe", BindingFlags.Public | BindingFlags.Instance) ?? throw new Exception("Failed getting call me method");
        public static readonly EndpointAttribute CallMeEndpointAttribute = CallMeMethod.GetCustomAttribute<EndpointAttribute>() ?? throw new Exception("Failed getting endpoint attribute");

        [Endpoint("/callme")]
        public HttpResponse<string> CallMe() => HttpResponse.Ok("Hello World!");

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

}
