using System.Net;

using Autumn.Annotations.Internal;
using Autumn.Http.Sessions;

namespace Autumn.Http.Interceptors;

[InternalSupportObject]
public sealed class AutumnHttpInterceptorChain : IHttpInterceptorChain {

    private readonly List<IHttpRequestInterceptor> requestInterceptors;
    private readonly List<IHttpResponseInterceptor> responseInterceptors;

    public IReadOnlyList<IHttpRequestInterceptor> RequestInterceptors => requestInterceptors;

    public IReadOnlyList<IHttpResponseInterceptor> ResponseInterceptors => responseInterceptors;

    public AutumnHttpInterceptorChain() {
        this.requestInterceptors = new List<IHttpRequestInterceptor>();
        this.responseInterceptors = new List<IHttpResponseInterceptor>();
    }

    public void AddFilter(IHttpRequestInterceptor interceptor) {
        this.requestInterceptors.Add(interceptor);
    }

    public void AddFilter(IHttpResponseInterceptor interceptor) {
        this.responseInterceptors.Add(interceptor);
    }

    public void AddFilter<T>(T interceptor) where T : IHttpRequestInterceptor, IHttpResponseInterceptor {
        this.AddFilter((IHttpRequestInterceptor)interceptor);
        this.AddFilter((IHttpResponseInterceptor)interceptor);
    }

    public void OnRequest(HttpListenerRequest request, IHttpSession session) {
        throw new NotImplementedException();
    }

    public void OnResponse(HttpListenerResponse response, IHttpSession session) {
        throw new NotImplementedException();
    }

}
