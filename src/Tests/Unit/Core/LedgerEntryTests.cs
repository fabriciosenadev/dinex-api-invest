namespace DinExApi.Tests;

public sealed class LedgerEntryTests
{
    [Fact]
    public void Should_Generate_Default_Description_For_Buy()
    {
        var entry = Create(
            type: LedgerEntryType.Buy,
            description: null,
            assetSymbol: "petr4",
            quantity: 10,
            unitPrice: 32.50m);

        Assert.True(entry.IsValid);
        Assert.Equal("Compra PETR4", entry.Description);
    }

    [Fact]
    public void Should_Generate_Default_Description_For_Fee()
    {
        var entry = Create(
            type: LedgerEntryType.Fee,
            description: null,
            grossAmount: 12.34m,
            netAmount: 12.34m,
            currency: "brl");

        Assert.True(entry.IsValid);
        Assert.StartsWith("Taxa BRL ", entry.Description);
    }

    [Fact]
    public void Should_Use_Provided_Description_When_Informed()
    {
        var entry = Create(
            type: LedgerEntryType.Income,
            description: "  Dividendo recebidon  ");

        Assert.Equal("Dividendo recebidon", entry.Description);
    }

    [Fact]
    public void Should_Be_Invalid_When_Buy_Has_No_Asset()
    {
        var entry = Create(
            type: LedgerEntryType.Buy,
            description: null,
            assetSymbol: null,
            quantity: 10,
            unitPrice: 32.50m);

        Assert.False(entry.IsValid);
        Assert.Contains(entry.Notifications, n => n.Key == "LedgerEntry.AssetSymbol");
    }

    [Fact]
    public void Should_Be_Invalid_When_Sell_Has_Invalid_Quantity_Or_UnitPrice()
    {
        var entry = Create(
            type: LedgerEntryType.Sell,
            description: null,
            assetSymbol: "ITSA4",
            quantity: 0,
            unitPrice: null);

        Assert.False(entry.IsValid);
        Assert.Contains(entry.Notifications, n => n.Key == "LedgerEntry.Quantity");
        Assert.Contains(entry.Notifications, n => n.Key == "LedgerEntry.UnitPriceAmount");
    }

    [Fact]
    public void Should_Be_Invalid_When_Currency_Has_Invalid_Length()
    {
        var entry = Create(
            type: LedgerEntryType.Adjustment,
            description: null,
            currency: "R");

        Assert.False(entry.IsValid);
        Assert.Contains(entry.Notifications, n => n.Key == "LedgerEntry.Currency");
    }

    private static LedgerEntry Create(
        LedgerEntryType type,
        string? description,
        decimal grossAmount = 100m,
        decimal netAmount = 100m,
        string currency = "BRL",
        string source = "manual",
        string? assetSymbol = null,
        decimal? quantity = null,
        decimal? unitPrice = null)
    {
        return new LedgerEntry(
            userId: Guid.NewGuid(),
            type: type,
            description: description,
            grossAmount: grossAmount,
            netAmount: netAmount,
            currency: currency,
            occurredAtUtc: DateTime.UtcNow,
            source: source,
            assetSymbol: assetSymbol,
            quantity: quantity,
            unitPriceAmount: unitPrice);
    }
}
