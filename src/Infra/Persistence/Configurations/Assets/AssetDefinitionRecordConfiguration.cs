namespace DinExApi.Infra;

internal sealed class AssetDefinitionRecordConfiguration : IEntityTypeConfiguration<AssetDefinitionRecord>
{
    public void Configure(EntityTypeBuilder<AssetDefinitionRecord> builder)
    {
        builder.ToTable("asset_definitions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Symbol).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150);
        builder.Property(x => x.Document).HasMaxLength(30);
        builder.Property(x => x.Country).HasMaxLength(40);
        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.Sector).HasMaxLength(80);
        builder.Property(x => x.Segment).HasMaxLength(80);
        builder.Property(x => x.ShareClass).HasMaxLength(40);
        builder.Property(x => x.CvmCode).HasMaxLength(30);
        builder.Property(x => x.FiiCategory).HasMaxLength(80);
        builder.Property(x => x.Administrator).HasMaxLength(150);
        builder.Property(x => x.Manager).HasMaxLength(150);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasIndex(x => new { x.UserId, x.Symbol }).IsUnique();
    }
}
