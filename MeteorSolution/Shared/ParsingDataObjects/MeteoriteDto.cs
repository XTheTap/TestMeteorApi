using System.ComponentModel.DataAnnotations;

public class MeteoriteDto
{
    [Required]
    public string Id { get; set; } = default!;

    [Required, MaxLength(128)]
    public string Name { get; set; } = default!;

    [Required]
    public string Nametype { get; set; } = default!;

    [Required]
    public string Recclass { get; set; } = default!;

    [Range(0, double.MaxValue, ErrorMessage = "Mass must be positive")]
    public string? Mass { get; set; }

    [Required]
    public string Fall { get; set; } = default!;

    public DateTime? Year { get; set; }

    public string? Reclat { get; set; }

    public string? Reclong { get; set; }

    public GeoLocation? Geolocation { get; set; }
}