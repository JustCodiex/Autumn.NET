using System.Net;

using Autumn.Http.Sessions;

namespace Autumn.Http.Interceptors;

/// <summary>
/// Interface for a HTTP request interceptor
/// </summary>
public interface IHttpRequestInterceptor {

    /// <summary>
    /// Intercepts an incoming HTTP request before the endpoint method is executed.
    /// </summary>
    /// <param name="request">The <see cref="HttpListenerRequest"/> that was intercepted.</param>
    /// <param name="session">The <see cref="IHttpSession"/> associated with the request (if any).</param>
    /// <returns>When <c>true</c> the interceptor chain continues. When <c>false</c> the chain is halted and the control is moved to <see cref="HandleError(HttpListenerResponse, IHttpSession?)"/>.</returns>
    bool Intercept(IHttpRequest request, IHttpSession? session);

    /// <summary>
    /// The error handler to be triggered if <see cref="Intercept(IHttpRequest, IHttpSession?)"/> returned <c>false</c>.
    /// </summary>
    /// <param name="response">The <see cref="HttpListenerResponse"/> to write faulty response data to.</param>
    /// <param name="session">The <see cref="IHttpSession"/> associated with the request (if any).</param>
    /// <returns>When value is <c>true</c> the request should proceed; Otherwise when <c>false</c> the response is returned and no further processing is applied.</returns>
    bool HandleError(HttpListenerResponse response, IHttpSession? session);

}
