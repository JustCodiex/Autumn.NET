using System.Collections.Generic;

using Autumn.Annotations;

using Budget.Model;

namespace Budget.Service;

[Service]
public class CurrencyService {

    public IList<Currency> Currencies { get; }

    public CurrencyService() {
        this.Currencies = new[] {
            new Currency("DKK", "Danske Kroner", (decimal)1.0, string.Empty),
            new Currency("EUR", "Euro", (decimal)1.0, "€")
        };
    }

}
