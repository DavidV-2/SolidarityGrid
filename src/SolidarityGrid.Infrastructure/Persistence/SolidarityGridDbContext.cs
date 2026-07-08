namespace SolidarityGrid.Infrastructure.Persistence;

public sealed class SolidarityGridDbContext : DbContext
{
    public SolidarityGridDbContext(
        DbContextOptions<SolidarityGridDbContext> options)
        : base(options)
    {
    }

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SolidarityGridDbContext).Assembly);

        base.OnModelCreating(modelBuilder);        
    }
}
