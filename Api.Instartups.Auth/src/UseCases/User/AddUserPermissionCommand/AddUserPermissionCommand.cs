using Api.Instartups.Auth.src.Interfaces.Command;

namespace Api.Instartups.Auth.src.UseCases.User.AddUserPermissionCommand;

public sealed record AddUserPermissionCommand(
        string UserId,
        string PermissionName
    ) : ICommand;
