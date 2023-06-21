using System;

using Autumn.Database.Annotations;

namespace Budget.Model;

[Row]
public class Purchase {

    [Column("id")]
    public int Id { get; set; }

    [Column("account")]
    public long AccountId { get; set; }

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("currency")]
    public string? Currency { get; set; }

    [Column("purchased_at")]
    public DateOnly Date { get; set; }

}
