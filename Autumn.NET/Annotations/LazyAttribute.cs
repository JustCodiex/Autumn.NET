namespace Autumn.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
public sealed class LazyAttribute : Attribute {

}
