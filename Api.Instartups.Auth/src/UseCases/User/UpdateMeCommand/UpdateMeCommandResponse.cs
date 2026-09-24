namespace Api.Instartups.Auth.src.UseCases.User.UpdateMeCommand;

public sealed record UpdateMeCommandResponse(
        string Id,
        string UserName,
        string Email,
        string? PhoneNumber
    );
