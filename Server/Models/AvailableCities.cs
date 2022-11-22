using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

[Table("AvailableCities")]
public record AvailableCity
{
    public int? Id { get; init; }
    public string? City { get; init; }
}