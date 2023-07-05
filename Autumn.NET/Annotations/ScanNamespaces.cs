namespace Autumn.Annotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class ScanNamespaces : Attribute {

    public string[] Namespaces { get; set; }

    public ScanNamespaces(string singleNamespace) {
        this.Namespaces = new string[] { singleNamespace };
    }

    public ScanNamespaces(params string[] namespaces) {
        this.Namespaces = namespaces;
    }

}
