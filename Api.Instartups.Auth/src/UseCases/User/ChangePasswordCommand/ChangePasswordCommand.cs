using Api.Instartups.Auth.src.Interfaces.Command;

namespace Api.Instartups.Auth.src.UseCases.User.ChangePasswordCommand;

public sealed record ChangePasswordCommand(
        string UserId,
        string CurrentPassword,
        string NewPassword
    ) : ICommand;
