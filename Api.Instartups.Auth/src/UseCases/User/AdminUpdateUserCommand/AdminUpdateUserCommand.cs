using Api.Instartups.Auth.src.Interfaces.Command;

namespace Api.Instartups.Auth.src.UseCases.User.AdminUpdateUserCommand;

public sealed record AdminUpdateUserCommand(
        string AdminUserId,
        string UserId,
        string UserName,
        string Email,
        string? PhoneNumber,
        string? NewPassword,
        bool IsLocked
    ) : ICommand;
