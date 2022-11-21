using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

[Table("CompanyDocuments")]
public record CompanyDocument
{
    public int? Id { get; init; }
    public string? Name { get; init; }
    public byte[]? Document { get; init; }
}