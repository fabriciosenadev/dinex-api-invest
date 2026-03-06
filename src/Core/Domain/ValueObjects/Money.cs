
namespace DinExApi.Core;

public readonly record struct Money
{
    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new DomainValidationException("Amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
        {
            throw new DomainValidationException("Currency must be a 3-letter code.");
        }

        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
    }

    public decimal Amount { get; }
    public string Currency { get; }
}
