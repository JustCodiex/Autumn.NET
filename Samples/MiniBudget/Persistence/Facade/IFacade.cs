using MiniBudget.Model;

namespace MiniBudget.Persistence.Facade;

public interface IFacade {
    
    IList<Purchase> GetAllPurchases();
    
    void RegisterPurchase(decimal amount, string desc, DateOnly date);

}
