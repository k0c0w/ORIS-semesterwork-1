using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

[Table("Tariffs")]
public record Tariff
{
    public int? Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public int? Car { get; init; }
}