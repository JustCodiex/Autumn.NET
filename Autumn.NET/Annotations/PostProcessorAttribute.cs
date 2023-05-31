namespace Autumn.Annotations;

[AttributeUsage(AttributeTargets.Method)]
public sealed class PostProcessorAttribute : Attribute {

    public string Method { get; }

    public PostProcessorAttribute(string method) {
        Method = method;
    }

}
