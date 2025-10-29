using Microsoft.EntityFrameworkCore;

namespace MeteorWorkerService;

public class AppDbContext : DbContext
{
    public DbSet<Meteorite> Meteorites => Set<Meteorite>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Meteorite>(entity =>
        {
            entity.ToTable("meteorites");
            entity.HasKey(e => e.ExternalId);
            entity.HasIndex(e => e.Year);
            entity.Property(e => e.RawJson).HasColumnType("jsonb");
        });
    }
}
