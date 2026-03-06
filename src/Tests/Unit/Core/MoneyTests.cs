
namespace DinExApi.Tests;

public sealed class MoneyTests
{
    [Fact]
    public void Should_Normalize_Currency_To_Uppercase()
    {
        var money = new Money(10m, "brl");

        Assert.Equal("BRL", money.Currency);
    }

    [Fact]
    public void Should_Throw_When_Currency_Is_Invalid()
    {
        Assert.Throws<DomainValidationException>(() => new Money(10m, "brazil"));
    }
}
