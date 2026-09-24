using Api.Instartups.Auth.src.Interfaces.Command;

namespace Api.Instartups.Auth.src.UseCases.User.RemoveUserPermissionCommand;

public sealed record RemoveUserPermissionCommand(
        string UserId,
        string PermissionName
    ) : ICommand;
