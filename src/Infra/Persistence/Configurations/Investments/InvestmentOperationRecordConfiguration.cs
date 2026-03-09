namespace DinExApi.Infra;

internal sealed class InvestmentOperationRecordConfiguration : IEntityTypeConfiguration<InvestmentOperationRecord>
{
    public void Configure(EntityTypeBuilder<InvestmentOperationRecord> builder)
    {
        builder.ToTable("investment_operations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AssetSymbol).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 6);
        builder.Property(x => x.UnitPriceAmount).HasPrecision(18, 6);

        builder.HasIndex(x => new { x.UserId, x.OccurredAtUtc });
        builder.HasIndex(x => x.AssetSymbol);
        builder.HasIndex(x => x.OccurredAtUtc);
    }
}
