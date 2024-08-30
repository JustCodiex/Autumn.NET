using System.Net;

using Autumn.Http.Sessions;

namespace Autumn.Http.Interceptors;

/// <summary>
/// 
/// </summary>
public interface IHttpInterceptorChain {
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="interceptor"></param>
    void AddInterceptor(IHttpRequestInterceptor interceptor);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="interceptor"></param>
    void AddInterceptor(IHttpResponseInterceptor interceptor);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="response"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    bool OnRequest(IHttpRequest request, HttpListenerResponse response, IHttpSession? session);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="responseObject"></param>
    /// <param name="response"></param>
    /// <param name="session"></param>
    /// <param name="finalResponseObject"></param>
    /// <returns></returns>
    bool OnResponse(object responseObject, HttpListenerResponse response, IHttpSession? session, out object finalResponseObject);

}
