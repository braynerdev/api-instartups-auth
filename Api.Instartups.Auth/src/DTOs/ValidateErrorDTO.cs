namespace Api.Instartups.Auth.DTOs;

public sealed record ValidateErrorDTO
{
    public string Field { get; init; }
    public string Code { get; init; }
    public string Message { get; init; }
}