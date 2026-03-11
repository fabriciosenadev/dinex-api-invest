namespace DinExApi.Infra;

internal sealed class AssetDefinitionRecordConfiguration : IEntityTypeConfiguration<AssetDefinitionRecord>
{
    public void Configure(EntityTypeBuilder<AssetDefinitionRecord> builder)
    {
        builder.ToTable("asset_definitions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Symbol).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasIndex(x => new { x.UserId, x.Symbol }).IsUnique();
    }
}
