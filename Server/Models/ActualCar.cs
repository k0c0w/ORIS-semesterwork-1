using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

[Table("CarPark")]
public record ActualCar
{
    public int? Id { get; init; }
    public int? Car { get; init; }
    public string? RegisterSign { get; init; }
    public int? City { get; init; }
    public bool? IsBusy { get; init; }
    public bool? IsReserved { get; init; }
}