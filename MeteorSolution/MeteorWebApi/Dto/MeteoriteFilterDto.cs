using System.ComponentModel.DataAnnotations;

public class MeteoriteFilterDto
{
    [Range(1000, 3000)]
    public int? YearFrom { get; set; }

    [Range(1000, 3000)]
    public int? YearTo { get; set; }

    public int? Recclass { get; set; }

    [MaxLength(100)]
    public string? NameContains { get; set; }

    public string SortBy { get; set; } = "year";
    public bool Desc { get; set; } = false;
}
