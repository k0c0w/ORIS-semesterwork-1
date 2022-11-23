namespace Server.Services.ServerServices;

public record class Session
{
    public Guid Id { get; init; }
    public int AccountId { get; init; }
    
    public DateTime CreateDateTime { get; init; }
}