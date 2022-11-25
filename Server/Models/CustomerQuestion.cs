using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

[Table("CustomersQuestions")]
public record CustomerQuestion
{
    public int? Id { get; init; }
    public int? UserId { get; init; }
    public string? ResponseEmail { get; init; }
    public string? Question { get; init; }
    
    public string? Name { get; init; }
    
    public string? Telephone { get; init; }
    
    public int? StatusCode { get; init; }
}