using System.ComponentModel.DataAnnotations;

public class Meteorite
{
    [Key]
    public string ExternalId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Nametype { get; set; }
    public string? Recclass { get; set; }
    public double? Mass { get; set; }
    public string? Fall { get; set; }
    public DateTime? Year { get; set; }
    public double? Reclat { get; set; }
    public double? Reclong { get; set; }
    public string RawJson { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
