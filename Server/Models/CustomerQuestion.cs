using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

[Table("CustomerQuestions")]
public record CustomerQuestion
{
    public int? Id { get; init; }
    public int? UserId { get; init; }
    public string? ResponseEmail { get; init; }
    public string? Question { get; init; }
    public QuestionStatus StatusCode { get; init; }
}