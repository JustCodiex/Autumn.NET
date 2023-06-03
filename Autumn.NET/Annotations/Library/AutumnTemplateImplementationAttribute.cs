namespace Autumn.Annotations.Library;

public class AutumnTemplateImplementationAttribute : ComponentAttribute {
    public Type TemplateType { get; }
    public AutumnTemplateImplementationAttribute(Type templateType) {
        this.TemplateType = templateType;
    }
}
