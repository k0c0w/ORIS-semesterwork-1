using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

[Table("Orders")]
public record Order
{
    public int? Id { get; init; }
    public int? UserId { get; init; }
    public int? TariffId { get; init; }
    public DateTime? SubscriptionStart { get; init; }
    public DateTime? SubscriptionEnd { get; init; }
    public int? CarId { get; init; }
    public int? CityId { get; init; }
    public bool? IsCancled { get; init; }
}