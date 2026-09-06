namespace Api.Instartups.Auth.DTOs;

public sealed record ValidateErrorDTO(
        string Field,
        string Code,
        IEnumerable<string> Message
    );