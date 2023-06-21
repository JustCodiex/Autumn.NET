using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

using Autumn.Annotations;

using Budget.Model;
using Budget.Persistence;
using Budget.Service;

namespace Budget.Pages.Model;

[Component(Scope = ComponentScope.Multiton)]
public class CreateTransactionPage : INotifyPropertyChanged {

    public enum CreateTransactionError {
        None,
        InvalidAmount,
        DatabaseError,
    }

    public record struct CreateTransactionResult(CreateTransactionError Error) {
        public readonly bool Success => Error == CreateTransactionError.None;
    }

    private readonly IFacade facade;
    private readonly CurrencyService currencyService;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Amount { get; set; } = "0";

    public string Description { get; set; } = "No Description";

    public DateTime Date { get; set; } = DateTime.Now;

    public ObservableCollection<Currency> Currencies => new (currencyService.Currencies);

    public ObservableCollection<Account> Accounts { get; }

    public Account? AccountId { get; set; }

    public Currency Currency { get; set; }

    public CreateTransactionPage([Inject] IFacade facade, [Inject] CurrencyService currencyService) {
        this.facade = facade;
        this.currencyService = currencyService;
        Accounts = new ObservableCollection<Account>();
        Task.Run(() => {
            Currency = Currencies[0];
            foreach (var acc in this.facade.GetAccounts()) {
                Accounts.Add(acc);
                AccountId ??= acc;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccountId)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Currency)));
        });
    }

    public async Task<CreateTransactionResult> CreateTransaction() {

        if (!decimal.TryParse(Amount, out var amount)) {
            return new CreateTransactionResult(CreateTransactionError.InvalidAmount);
        }

        var updated = await Task.Run(() => facade.CreatePurchase(new Purchase() { 
            Amount = amount,
            Description = this.Description,
            Date = DateOnly.FromDateTime(Date),
            Currency = this.Currency.Identifier,
            AccountId = this.AccountId!.Id,
        }));
        if (updated == 0) {
            return new CreateTransactionResult(CreateTransactionError.DatabaseError);
        }

        return new CreateTransactionResult(CreateTransactionError.None);

    }

}
