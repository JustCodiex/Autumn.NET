using Autumn.Database;

using MiniBudget.Model;

namespace MiniBudget.Persistence.Postgres;

public class TransactionDAO {

    private readonly DatabaseTemplate databaseTemplate;

    public TransactionDAO(DatabaseTemplate databaseTemplate) {
        this.databaseTemplate = databaseTemplate;
    }

    public IList<Purchase> GetAllPurchases() {
        string sql = """
            SELECT * FROM purchase
            """;
        return databaseTemplate.Query<Purchase>(sql);
    }

    public void RegisterPurchase(decimal amount, string desc, DateOnly date) {
        string sql = """
            INSERT INTO purchase (amount, description, purchased_at)
            VALUES (?, ?, ?)
            """;
        databaseTemplate.Update(sql, amount, desc, date);
    }

}
