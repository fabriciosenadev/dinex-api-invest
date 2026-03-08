
namespace DinExApi.Infra;

public sealed class DinExDbContext(DbContextOptions<DinExDbContext> options) : DbContext(options)
{
    public DbSet<InvestmentOperationRecord> InvestmentOperations => Set<InvestmentOperationRecord>();
    public DbSet<LedgerEntryRecord> LedgerEntries => Set<LedgerEntryRecord>();
    public DbSet<UserRecord> Users => Set<UserRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DinExDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
