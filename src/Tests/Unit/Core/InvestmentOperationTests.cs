namespace DinExApi.Tests;

public sealed class InvestmentOperationTests
{
    [Fact]
    public void Should_Normalize_Asset_And_Keep_Utc_Date()
    {
        var operation = new InvestmentOperation(
            userId: Guid.NewGuid(),
            assetSymbol: " petr4 ",
            type: OperationType.Buy,
            quantity: 10,
            unitPrice: new Money(10m, "brl"),
            occurredAtUtc: DateTime.Now);

        Assert.True(operation.IsValid);
        Assert.Equal("PETR4", operation.AssetSymbol);
        Assert.Equal(DateTimeKind.Utc, operation.OccurredAtUtc.Kind);
    }

    [Fact]
    public void Should_Be_Invalid_When_UserId_Is_Empty()
    {
        var operation = new InvestmentOperation(
            userId: Guid.Empty,
            assetSymbol: "PETR4",
            type: OperationType.Buy,
            quantity: 10,
            unitPrice: new Money(10m, "BRL"),
            occurredAtUtc: DateTime.UtcNow);

        Assert.False(operation.IsValid);
        Assert.Contains(operation.Notifications, n => n.Key == "InvestmentOperation.UserId");
    }

    [Fact]
    public void Should_Be_Invalid_When_Quantity_Is_Invalid()
    {
        var operation = new InvestmentOperation(
            userId: Guid.NewGuid(),
            assetSymbol: "PETR4",
            type: OperationType.Sell,
            quantity: 0,
            unitPrice: new Money(0m, "BRL"),
            occurredAtUtc: DateTime.UtcNow);

        Assert.False(operation.IsValid);
        Assert.Contains(operation.Notifications, n => n.Key == "InvestmentOperation.Quantity");
    }
}
