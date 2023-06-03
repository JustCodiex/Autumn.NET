using Autumn.Database.Annotations;

namespace MiniBudget.Model;

[Row]
public class Purchase {

    [Column("id")]
    public int Id { get; set; }

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("purchased_at")]
    public DateOnly Date { get; set; }

}
