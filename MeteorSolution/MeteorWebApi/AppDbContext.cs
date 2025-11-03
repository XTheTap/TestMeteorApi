using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v
            );

            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );

            entity.Property(m => m.Year)
                .HasConversion(nullableDateTimeConverter)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.CreatedAt)
                .HasConversion(dateTimeConverter)
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.UpdatedAt)
                .HasConversion(dateTimeConverter)
                .HasColumnType("timestamp with time zone");
        });
    }   
}