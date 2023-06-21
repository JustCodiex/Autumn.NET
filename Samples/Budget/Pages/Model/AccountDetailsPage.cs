using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

using Autumn.Annotations;

using Budget.Model;
using Budget.Persistence;

namespace Budget.Pages.Model;

[Component(Scope = ComponentScope.Multiton)]
public class AccountDetailsPage {

    public record AccountTransaction(string Description, decimal Amount, string Currency, DateOnly Date);

    private readonly Dispatcher dispatcher;
    private readonly IFacade facade;

    public Account Account { get; }

    public ObservableCollection<AccountTransaction> Transactions { get; }

    public AccountDetailsPage([Inject] Dispatcher dispatcher, [Inject] IFacade facade, Account account) {
        this.Account = account;
        this.facade = facade;
        this.dispatcher = dispatcher;
        this.Transactions = new ObservableCollection<AccountTransaction>();
        Task.Run(() => {
            var purchases = this.facade.GetAccountPurchases(account.Id, DateOnly.FromDateTime(DateTime.Now.AddDays(-30)));
            var orderedList = new List<AccountTransaction>();
            foreach (var purchase in purchases) {
                orderedList.Add(MapPurchaseToTransaction(purchase));
            }
            this.dispatcher.Invoke(() => {
                orderedList.OrderBy(x => x.Date).ToList().ForEach(this.Transactions.Add);
            });
        });

    }

    private AccountTransaction MapPurchaseToTransaction(Purchase purchase) {
        return new AccountTransaction(purchase.Description ?? "No Description", -purchase.Amount, purchase.Currency ?? Account.Currency!, purchase.Date);
    }

}
