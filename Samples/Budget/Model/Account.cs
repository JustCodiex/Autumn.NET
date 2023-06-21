using Autumn.Database.Annotations;

namespace Budget.Model;

[Row]
public sealed class Account {

    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("main_currency")]
    public string? Currency { get; set; }

    public decimal Balance { get; set; }

    public decimal Income { get; set; }

    public decimal Expenditures { get; set; }

}
