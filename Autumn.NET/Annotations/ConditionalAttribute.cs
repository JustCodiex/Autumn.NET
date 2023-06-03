namespace Autumn.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class ConditionalAttribute : Attribute {

    public string? HasValue { get; set; }

    public string Property { get; }

    public ConditionalAttribute(string property) { 
        this.Property = property;
    }

}
