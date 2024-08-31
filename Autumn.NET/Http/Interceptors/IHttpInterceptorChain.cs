using System.Net;

using Autumn.Http.Sessions;

namespace Autumn.Http.Interceptors;

/// <summary>
/// Interface representing a chain of interceptors.
/// </summary>
public interface IHttpInterceptorChain {
    
    /// <summary>
    /// Add a request interceptor to the interceptor chain.
    /// </summary>
    /// <param name="interceptor">The <see cref="IHttpRequestInterceptor"/> to add.</param>
    void AddInterceptor(IHttpRequestInterceptor interceptor);

    /// <summary>
    /// Add a response interceptor to the interceptor chain.
    /// </summary>
    /// <param name="interceptor">The <see cref="IHttpResponseInterceptor"/> to add.</param>
    void AddInterceptor(IHttpResponseInterceptor interceptor);

    /// <summary>
    /// Handles the interception of a new request.
    /// </summary>
    /// <param name="request">The <see cref="IHttpRequest"/> that triggered the interception chain</param>
    /// <param name="response">The <see cref="HttpListenerResponse"/> to write to if an interceptor halts the chain</param>
    /// <param name="session">The <see cref="IHttpSession"/> associated with the request</param>
    /// <returns>When <c>true</c> all interceptors successfully executed and allows the request to proceed. When <c>false</c> the request was intercepted and returned early.</returns>
    bool OnRequest(IHttpRequest request, HttpListenerResponse response, IHttpSession? session);

    /// <summary>
    /// Handles the interception of a request response
    /// </summary>
    /// <param name="responseObject">The result of the endpoint logic</param>
    /// <param name="response">The response object to attach or write data to</param>
    /// <param name="session">The <see cref="IHttpSession"/> associated with the response</param>
    /// <param name="finalResponseObject">The final response object to return</param>
    /// <returns>When <c>true</c> all interceptors successfully executed and allows the ending response logic to proceed. When <c>false</c> an interceptor has already responded.</returns>
    bool OnResponse(object responseObject, HttpListenerResponse response, IHttpSession? session, out object finalResponseObject);

}
