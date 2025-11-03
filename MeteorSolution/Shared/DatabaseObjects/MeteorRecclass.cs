using System.ComponentModel.DataAnnotations;

public class MeteorRecclass
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = default!;
}