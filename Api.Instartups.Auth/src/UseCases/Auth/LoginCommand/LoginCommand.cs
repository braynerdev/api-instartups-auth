using Api.Instartups.Auth.src.Interfaces.Command;

namespace Api.Instartups.Auth.UseCases.Auth.LoginCommand;

public sealed record LoginCommand(
    string UserNameOrEmail,
    string Password) : ICommand;