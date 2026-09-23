using Api.Instartups.Auth.src.Interfaces.Command;

namespace Api.Instartups.Auth.src.UseCases.User.UpdateMeCommand;

public sealed record UpdateMeCommand(
        string UserId,
        string UserName,
        string Email,
        string? PhoneNumber
    ) : ICommand;
