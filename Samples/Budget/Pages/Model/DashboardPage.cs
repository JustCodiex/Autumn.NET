using System;
using System.Collections.ObjectModel;
using System.Linq;

using Autumn.Annotations;

using Budget.Model;
using Budget.Persistence;

namespace Budget.Pages.Model;

[Component]
public class DashboardPage {

    private readonly IFacade facade;

    private readonly ObservableCollection<Account> accounts;

    public ObservableCollection<Account> Accounts => accounts;

    public string TotalExpenditure => accounts.Select(x => x.Expenditures).Sum().ToString("0.00");

    public string TotalIncome => accounts.Select(x => x.Income).Sum().ToString("0.00");

    public string TotalBalance => accounts.Select(x => x.Balance).Sum().ToString("0.00");

    public DashboardPage([Inject] IFacade facade) {
        this.facade = facade;
        this.accounts = new ObservableCollection<Account>();
    }

    public void UpdateAccounts() {
        accounts.Clear();
        foreach (var acc in facade.GetAccounts()) {
            accounts.Add(GetProperAccount(acc));
        }
    }

    private Account GetProperAccount(Account account) {

        AccountStatus status = facade.GetAccountStatus(account.Id, DateOnly.FromDateTime(DateTime.Now));
        decimal balance = status.StartBalance + status.Change;
        decimal expenditures = facade.GetAccountPurchases(account.Id, status.Date).Select(x => x.Amount).Sum();
        decimal income = 0;

        return new Account() { 
            Id = account.Id, 
            Balance = balance + income - expenditures,
            Name = account.Name,
            Description = account.Description,
            Currency = account.Currency,
            Expenditures = expenditures,
            Income = income,
        };

    }

}
