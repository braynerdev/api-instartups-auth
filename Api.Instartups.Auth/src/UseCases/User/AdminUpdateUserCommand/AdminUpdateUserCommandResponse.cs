namespace Api.Instartups.Auth.src.UseCases.User.AdminUpdateUserCommand;

public sealed record AdminUpdateUserCommandResponse(
        string Id,
        string UserName,
        string Email,
        string? PhoneNumber,
        bool IsLocked
    );
