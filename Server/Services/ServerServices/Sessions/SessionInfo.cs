namespace Server.Services.ServerServices;

public record class SessionInfo
{
    public Guid Guid { get; init; }
    
    public bool LongLife { get; init; }
}