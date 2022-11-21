using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

[Table("PersonalInfo")]
public record PersonalInfo
{
    public int? Id { get; init; }
    public int? UserId { get; init; }
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public uint? TelephoneNumber { get; init; }
    public int? DriverLicense { get; init; }
    public ulong? Passport { get; init; }
    public ulong? CardNumber { get; init; }
    public string? CardName { get; init; }
    public int? CVC { get; init; }
}