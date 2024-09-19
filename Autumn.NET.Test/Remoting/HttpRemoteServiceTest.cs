using System.Net;

using Autumn.Annotations;
using Autumn.Context;
using Autumn.Http;
using Autumn.Http.Annotations;
using Autumn.Proxying;
using Autumn.Remoting;
using Autumn.Test.TestHelpers;

namespace Autumn.Test.Remoting;

public class HttpRemoteServiceTest {

    [Service]
    public class CalculatorService {

        [Endpoint("/add")]
        public virtual int Add(int x, int y) => x + y;

    }

    [Component]
    public class RemoteCalculator([Inject] HttpRemoteService<CalculatorService> remoteService) {

        public HttpRemoteService<CalculatorService> Service => remoteService;

        public int Add(int x, int y) => Service.Service.Add(x, y);

    }

    [Fact]
    public void CanCreateCalculator() {

        // Init context
        var appContext = new AutumnAppContext();
        appContext.RegisterComponent(typeof(HttpRemoteService<>));
        appContext.RegisterComponent<HttpClient>();
        appContext.RegisterComponent(ConfigHelper.ConfigOf(""));
        appContext.RegisterComponent<AutumnHttpClient>();
        appContext.RegisterService<CalculatorService>();
        appContext.RegisterService<RemoteCalculator>();

        var remotableService = appContext.GetInstanceOf<RemoteCalculator>();
        Assert.NotNull(remotableService);
        Assert.NotNull(remotableService.Service);

    }

    [Fact]
    public void CanInvokeCalculator() {

        // Hook a mocked interceptor
        var proxyInterceptor = Proxy.CreateProxy<IHttpClient>(
            (method, args) => new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("12")
            });

        // Init context
        var appContext = new AutumnAppContext();
        appContext.RegisterComponent(typeof(HttpRemoteService<>));
        appContext.RegisterComponent(ConfigHelper.ConfigOf(""));
        appContext.RegisterComponent(proxyInterceptor);
        appContext.RegisterService<CalculatorService>();
        appContext.RegisterService<RemoteCalculator>();

        var remotableService = appContext.GetInstanceOf<RemoteCalculator>();
        Assert.NotNull(remotableService);
        Assert.NotNull(remotableService.Service);

        var result = remotableService.Add(5, 7);
        Assert.Equal(12, result);

    }

}
