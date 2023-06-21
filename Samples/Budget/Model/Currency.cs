namespace Budget.Model;

public record struct Currency(string Identifier, string Name, decimal ExchangeRateToEuro, string Symbol) {
    public override readonly string ToString() => Identifier;
}
