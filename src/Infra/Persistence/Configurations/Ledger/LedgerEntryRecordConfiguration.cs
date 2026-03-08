namespace DinExApi.Infra;

internal sealed class LedgerEntryRecordConfiguration : IEntityTypeConfiguration<LedgerEntryRecord>
{
    public void Configure(EntityTypeBuilder<LedgerEntryRecord> builder)
    {
        builder.ToTable("ledger_entries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).HasMaxLength(160).IsRequired();
        builder.Property(x => x.AssetSymbol).HasMaxLength(20);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ReferenceId).HasMaxLength(120);
        builder.Property(x => x.Metadata).HasMaxLength(4000);
        builder.Property(x => x.Quantity).HasPrecision(18, 6);
        builder.Property(x => x.UnitPriceAmount).HasPrecision(18, 6);
        builder.Property(x => x.GrossAmount).HasPrecision(18, 6);
        builder.Property(x => x.NetAmount).HasPrecision(18, 6);

        builder.HasIndex(x => new { x.UserId, x.OccurredAtUtc });
        builder.HasIndex(x => x.AssetSymbol);
        builder.HasIndex(x => new { x.UserId, x.Source, x.ReferenceId }).IsUnique();
    }
}
