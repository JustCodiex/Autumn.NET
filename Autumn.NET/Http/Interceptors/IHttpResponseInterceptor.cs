using System.Net;

using Autumn.Http.Sessions;

namespace Autumn.Http.Interceptors;

/// <summary>
/// Interface for a HTTP response interceptor
/// </summary>
public interface IHttpResponseInterceptor {

    /// <summary>
    /// Intercepts the output of the interceptor
    /// </summary>
    /// <param name="interceptedResult">The intercepted result.</param>
    /// <param name="response">The response that can be written to.</param>
    /// <param name="session">The session of the HTTP request that was intercepted</param>
    /// <param name="result">The output of the interceptor</param>
    /// <returns>When <c>true</c> the interception chain continues, using the value of <paramref name="result"/> as <paramref name="interceptedResult"/>. A value of <c>false</c> will halt the interception chain.</returns>
    bool Intercept(object interceptedResult, HttpListenerResponse response, IHttpSession? session, out object result);

}
