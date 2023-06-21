using System;

using Autumn.Database.Annotations;

namespace Budget.Model;

[Row]
public class AccountStatus {

	[Column("id")]
	public int Id { get; set; }

	[Column("account")]
	public long AccountId { get; set; }

	[Column("start_of_month")]
	public decimal StartBalance { get; set; }

	[Column("end_of_month")]
	public decimal EndBalance { get; set; }

	public decimal Change => StartBalance + EndBalance;

	[Column("dated_at")]
	public DateOnly Date { get; set; }

}
