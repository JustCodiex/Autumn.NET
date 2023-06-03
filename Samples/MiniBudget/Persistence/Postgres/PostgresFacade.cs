using Autumn.Annotations;
using Autumn.Database;

using MiniBudget.Model;
using MiniBudget.Persistence.Facade;

namespace MiniBudget.Persistence.Postgres;

[Component, Conditional("autumn.datasource.type", HasValue = "postgres")]
public class PostgresFacade : IFacade {

    private readonly DatabaseTemplate template;
    private readonly TransactionDAO transactionDAO;

    public PostgresFacade([Inject] DatabaseTemplate postgresDatabaseTemplate) { 
        this.template = postgresDatabaseTemplate;
        this.transactionDAO = new TransactionDAO(postgresDatabaseTemplate);
    }

    public IList<Purchase> GetAllPurchases() => this.transactionDAO.GetAllPurchases();

    public void RegisterPurchase(decimal amount, string desc, DateOnly date) => this.transactionDAO.RegisterPurchase(amount, desc, date);

}
