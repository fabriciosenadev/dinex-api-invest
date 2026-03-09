namespace DinExApi.Infra;

internal sealed class CorporateEventRecordConfiguration : IEntityTypeConfiguration<CorporateEventRecord>
{
    public void Configure(EntityTypeBuilder<CorporateEventRecord> builder)
    {
        builder.ToTable("corporate_events");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceAssetSymbol).HasMaxLength(20).IsRequired();
        builder.Property(x => x.TargetAssetSymbol).HasMaxLength(20);
        builder.Property(x => x.Factor).HasPrecision(18, 6);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasIndex(x => new { x.UserId, x.EffectiveAtUtc });
        builder.HasIndex(x => x.SourceAssetSymbol);
    }
}
