using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Meteorite> Meteorites => Set<Meteorite>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<Meteorite>(entity =>
        {
            entity.ToTable("meteorites");
            entity.HasKey(e => e.ExternalId);
            entity.HasIndex(e => e.Year);
            entity.Property(e => e.RawJson).HasColumnType("jsonb");

            entity.Property(m => m.Year)
                .HasConversion(
                    v => v, 
                    v => v.HasValue 
                        ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) 
                        : (DateTime?)null
                )
                .HasColumnType("timestamp with time zone");
            
            entity.Property(e => e.CreatedAt)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                )
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.UpdatedAt)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                )
                .HasColumnType("timestamp with time zone");
        });
    }   
}