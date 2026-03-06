namespace DinExApi.Infra;

internal sealed class UserRecordConfiguration : IEntityTypeConfiguration<UserRecord>
{
    public void Configure(EntityTypeBuilder<UserRecord> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Password).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ActivationCode).HasMaxLength(30);
        builder.Property(x => x.ActivationCodeFailedAttempts).HasDefaultValue(0);
        builder.Property(x => x.PasswordResetCode).HasMaxLength(30);
        builder.Property(x => x.RefreshTokenHash).HasMaxLength(200);
        builder.Property(x => x.LoginFailedAttempts).HasDefaultValue(0);

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.RefreshTokenHash).IsUnique();
        builder.HasIndex(x => x.PasswordResetCode);
    }
}
