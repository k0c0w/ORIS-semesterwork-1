using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

[Table("Cars")]
public record CarModel
{
    public int? Id { get; init; }
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public string? Description { get; init; }
    public string? ImageSource { get; init; }
}