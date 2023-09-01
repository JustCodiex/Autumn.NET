namespace Autumn.Database.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class ColumnAttribute : Attribute {

    public string? ColumnName { get; }

    public bool PrimaryKey { get; set; }

    public ColumnAttribute() { }

    public ColumnAttribute(string? columnName) {
        ColumnName = columnName;
    }

}
