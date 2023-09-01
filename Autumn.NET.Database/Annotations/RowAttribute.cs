namespace Autumn.Database.Annotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class RowAttribute : Attribute {
    public string? RelationName { get; set; }
}
