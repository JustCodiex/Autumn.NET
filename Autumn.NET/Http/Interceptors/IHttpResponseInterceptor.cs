using System.Net;

using Autumn.Http.Sessions;

namespace Autumn.Http.Interceptors;

public interface IHttpResponseInterceptor {

    bool Intercept(HttpListenerResponse response, IHttpSession session);

}
