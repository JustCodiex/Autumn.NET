using System;
using System.Collections.Generic;

using Autumn.Annotations;
using Autumn.Database;

using Budget.Model;

namespace Budget.Persistence.Postgres;

[Component]
public class PostgresFacade : IFacade {

    private readonly DatabaseTemplate databaseTemplate;

    public PostgresFacade([Inject] DatabaseTemplate postgresDatabaseTemplate) { 
        this.databaseTemplate = postgresDatabaseTemplate;
    }

    public IList<Account> GetAccounts()
        => databaseTemplate.Query<Account>("""
            SELECT * FROM account
            """);

    public int CreatePurchase(Purchase purchase)
        => databaseTemplate.Update("""
            INSERT INTO purchase (account, amount, currency, description, purchased_at)
            VALUES (?, ?, ?, ?, ?)
            """, purchase.AccountId, purchase.Amount, purchase.Currency, purchase.Description, purchase.Date);

    public AccountStatus GetAccountStatus(long accountId, DateOnly date) {
        var latest = databaseTemplate.Query<AccountStatus>("""
            SELECT * FROM account_status WHERE dated_at >= ? ORDER BY dated_at DESC
            """, date.AddMonths(-2));
        return latest.Count > 0 ? latest[0] : new AccountStatus() { AccountId = accountId, Date = date.AddMonths(-1), StartBalance = 0, EndBalance = 0 };
    }

    public IList<Purchase> GetAccountPurchases(long id, DateOnly date)
        => databaseTemplate.Query<Purchase>("""
            SELECT * FROM purchase WHERE purchased_at >= ? AND account = ?
            """, date, id);

}
