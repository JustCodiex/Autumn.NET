namespace Autumn.Http.Annotations;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class ParameterAttribute : Attribute {
    public string Name { get; }
    public ParameterAttribute(string name) {
        this.Name = name;
    }
}
