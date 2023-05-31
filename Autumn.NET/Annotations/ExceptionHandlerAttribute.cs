namespace Autumn.Annotations;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ExceptionHandlerAttribute : Attribute {

    public string Method { get; }

    public Type? Target { get; }

    public Type[] Filter { get; }

    public ExceptionHandlerAttribute(string method, params Type[] filter) { 
        this.Method = method;
        this.Filter = filter;
    }

}
