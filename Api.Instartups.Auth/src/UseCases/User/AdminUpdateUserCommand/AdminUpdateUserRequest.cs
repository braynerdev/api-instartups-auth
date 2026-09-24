namespace Api.Instartups.Auth.src.UseCases.User.AdminUpdateUserCommand;

public sealed record AdminUpdateUserRequest(
        string UserName,
        string Email,
        string? PhoneNumber,
        string? NewPassword,
        bool IsLocked
    );
