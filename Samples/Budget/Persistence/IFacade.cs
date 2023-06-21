using System;
using System.Collections.Generic;

using Budget.Model;

namespace Budget.Persistence;

public interface IFacade {

    int CreatePurchase(Purchase purchase);

    AccountStatus GetAccountStatus(long accountId, DateOnly date);

    IList<Account> GetAccounts();

    IList<Purchase> GetAccountPurchases(long id, DateOnly date);

}
