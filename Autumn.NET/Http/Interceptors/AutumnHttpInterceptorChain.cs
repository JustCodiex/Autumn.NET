using System.Net;
using System.Reflection;

using Autumn.Annotations;
using Autumn.Annotations.Internal;
using Autumn.Http.Annotations;
using Autumn.Http.Sessions;

namespace Autumn.Http.Interceptors;

/// <summary>
/// Class representing a chain of interceptors.
/// </summary>
[InternalSupportObject]
public sealed class AutumnHttpInterceptorChain : IHttpInterceptorChain {

    private sealed class DuplicateIndexComparer : IComparer<int> { // https://stackoverflow.com/a/21886340
        public int Compare(int x, int y) {
            int result = x.CompareTo(y);
            if (result == 0)
                return 1; // Handle equality as being greater. Note: this will break Remove(key) or
            else          // IndexOfKey(key) since the comparer never returns 0 to signal key equality
                return result;
        }
    }

    private readonly SortedList<int, IHttpRequestInterceptor> requestInterceptors;
    private readonly SortedList<int, IHttpResponseInterceptor> responseInterceptors;

    /// <summary>
    /// Get the list of request interceptors.
    /// </summary>
    public IReadOnlyList<IHttpRequestInterceptor> RequestInterceptors => requestInterceptors.Values.AsReadOnly();

    /// <summary>
    /// Get the list of response intercepters.
    /// </summary>
    public IReadOnlyList<IHttpResponseInterceptor> ResponseInterceptors => responseInterceptors.Values.AsReadOnly();

    /// <summary>
    /// Initialize a new <see cref="AutumnHttpInterceptorChain"/> instance.
    /// </summary>
    public AutumnHttpInterceptorChain() {
        var comparer = new DuplicateIndexComparer();
        this.requestInterceptors = new(comparer);
        this.responseInterceptors = new(comparer);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="interceptor"></param>
    public void AddInterceptor(IHttpRequestInterceptor interceptor) {
        int order = GetOrder(interceptor.GetType(), this.requestInterceptors.LastOrDefault().Key + 1, t => this.responseInterceptors.IndexOfValue(this.responseInterceptors.FirstOrDefault(x => x.Value.GetType() == t).Value));
        this.requestInterceptors.Add(order, interceptor);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="interceptor"></param>
    public void AddInterceptor(IHttpResponseInterceptor interceptor) {
        int order = GetOrder(interceptor.GetType(), this.responseInterceptors.LastOrDefault().Key + 1, t => this.responseInterceptors.IndexOfValue(this.responseInterceptors.FirstOrDefault(x => x.Value.GetType() == t).Value));
        this.responseInterceptors.Add(order, interceptor);
    }

    private static int GetOrder(Type type, int lastIndex, Func<Type, int> indexOf) {

        int byOrderAttrib = OrderAttribute.GetOrder(type);
        if (byOrderAttrib >= 0)
            return byOrderAttrib;

        int before = type.GetCustomAttribute<InterceptBeforeAttribute>() is InterceptBeforeAttribute interceptBefore ? indexOf(interceptBefore.InterceptBefore) : -1;
        if (before >= 0) 
            return before - 1;

        int after = type.GetCustomAttribute<InterceptAfterAttribute>() is InterceptAfterAttribute interceptAfter ? indexOf(interceptAfter.InterceptAfter) : -1;
        if (after >= 0) // TODO: Check interceptAfter.InterceptAfter != type
            return after + 1;

        return lastIndex;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="interceptor"></param>
    public void AddInterceptor<T>(T interceptor) where T : IHttpRequestInterceptor, IHttpResponseInterceptor {
        AddInterceptor((IHttpRequestInterceptor)interceptor);
        AddInterceptor((IHttpResponseInterceptor)interceptor);
    }

    /// <inheritdoc/>
    public bool OnRequest(IHttpRequest request, HttpListenerResponse response, IHttpSession? session) {
        bool hasHandled = true; // flag telling outer context if call should proceed
        var it = this.requestInterceptors.GetEnumerator();
        while (it.MoveNext()) {
            IHttpRequestInterceptor interceptor = it.Current.Value;
            if (!interceptor.Intercept(request, session)) {
                hasHandled = !interceptor.HandleError(response, session);
                break;
            }
        }
        return hasHandled;
    }

    /// <inheritdoc/>
    public bool OnResponse(object responseObject, HttpListenerResponse response, IHttpSession? session, out object finalResponseObject) {
        finalResponseObject = responseObject;
        var it = this.responseInterceptors.GetEnumerator();
        while (it.MoveNext()) { 
            IHttpResponseInterceptor interceptor = it.Current.Value;
            if (!interceptor.Intercept(finalResponseObject, response, session, out finalResponseObject)) { 
                return false;
            }
        }
        return true;
    }

}
