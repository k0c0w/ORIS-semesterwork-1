namespace Server.Services;

public record class OperationResultDto
{
    public bool Success { get; init; }

    public InputError[] Errors { get; init; } = Array.Empty<InputError>();
}