using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

[Table("Users")]
public record class User
{
    public int? Id { get; init; }
    
    public string? FirstName { get; init; }
    
    public string? Email { get; init; }
    
    public string? Password { get; init; }
    
    public DateTime? BirthDate { get; init; }

    public bool? DataProcessingAgreement { get; init; }

    public bool? IsVerified { get; init; }
}